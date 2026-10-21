using System;
using System.Collections.Generic;
using GTA;
using GTA4Nemesis.Model;

namespace GTA4Nemesis.Core
{
    /// <summary>
    /// Nemesis system: persistent named enemies, vendettas, promotions and the
    /// player's personal nemesis (inspired by Shadow of Mordor / War).
    /// Death detection is done by polling isAlive (ScriptHookDotNet has no
    /// death event on Ped).
    /// </summary>
    public class NemesisSystem
    {
        readonly GameState _state;
        readonly Config _cfg;
        readonly Dictionary<string, Ped> _memberPed = new Dictionary<string, Ped>(); // member id -> live ped

        bool _playerWasAlive = true;
        DateTime _nextEncounter = DateTime.MinValue;

        public NemesisSystem(GameState state, Config cfg)
        {
            _state = state;
            _cfg = cfg;
        }

        public void Tick()
        {
            DateTime now = DateTime.UtcNow;

            CheckPlayerDeath();

            if (_cfg.NemesisEnabled && now >= _nextEncounter)
            {
                _nextEncounter = now.AddSeconds(Helpers.Rng.Next(90, 240));
                MaybeSpawnEncounter();
            }

            CheckMemberDeaths();
        }

        /// <summary>Spawn a member as a live ped at a position, armed and hostile.</summary>
        public Ped SpawnMemberAt(string memberId, Vector3 pos, bool hostileToPlayer)
        {
            GangMember m = _state.GetMember(memberId);
            if (m == null || !m.Alive) return null;
            if (_memberPed.ContainsKey(memberId)) return _memberPed[memberId]; // already live

            Ped p = Helpers.SpawnPed(m.ModelName, pos);
            if (p == null) return null;

            Helpers.SetHealth(p, m.EffectiveMaxHealth());
            Helpers.Safe(() => p.Accuracy = m.EffectiveAccuracy());
            Helpers.ArmPed(p, m.CombatTier());
            Helpers.Safe(() => p.Armor = m.Rank >= 5 ? 100 + m.Rank * 10 : 0);

            if (hostileToPlayer) Helpers.SetHostileToPlayer(p);
            else Helpers.SetFriendlyToPlayer(p);

            Helpers.Safe(() => p.Task.AlwaysKeepTask = true);
            Helpers.Safe(() => p.Task.FightAgainstHatedTargets(100f));

            _memberPed[memberId] = p;
            m.LastX = pos.X; m.LastY = pos.Y; m.LastZ = pos.Z;
            return p;
        }

        public Ped GetLivePed(string memberId)
        {
            Ped p;
            return _memberPed.TryGetValue(memberId, out p) ? p : null;
        }

        void CheckMemberDeaths()
        {
            List<string> dead = new List<string>();
            foreach (KeyValuePair<string, Ped> kv in _memberPed)
                if (kv.Value == null || !kv.Value.isAlive) dead.Add(kv.Key);

            foreach (string id in dead)
            {
                Ped p;
                _memberPed.TryGetValue(id, out p);
                _memberPed.Remove(id);
                HandleMemberDeath(id, p);
            }
        }

        void CheckPlayerDeath()
        {
            Ped player = Helpers.Player;
            if (player == null) return;
            bool alive = player.isAlive;
            if (_playerWasAlive && !alive) HandlePlayerDeath(player);
            _playerWasAlive = alive;
        }

        void HandleMemberDeath(string memberId, Ped p)
        {
            GangMember m = _state.GetMember(memberId);
            if (m == null || !m.Alive) return;
            m.Alive = false;

            Vector3 deathPos = new Vector3(m.LastX, m.LastY, m.LastZ);
            if (p != null) Helpers.Safe(() => deathPos = p.Position);

            bool playerKill = false;
            Ped player = Helpers.Player;
            if (player != null && player.isAlive)
            {
                Vector3 pp = new Vector3(0f, 0f, 0f);
                Helpers.Safe(() => pp = player.Position);
                playerKill = Helpers.Near(pp, deathPos, 70f);
            }

            if (playerKill)
            {
                int kills = _state.GetStatInt("nemesis_kills", 0) + 1;
                _state.SetStatInt("nemesis_kills", kills);

                int reward = 200 + m.Rank * 60 + m.Level * 20;
                Helpers.Safe(() => Game.LocalPlayer.Money += reward);
                Helpers.ShowMessage("Вы устранили " + m.Name + " (+$" + reward + ")", 3500);

                if (m.IsNemesis)
                {
                    m.IsNemesis = false;
                    Helpers.ShowMessage("НЕМЕЗИДА ПОВЕРЖЕН: " + m.Name, 6000);
                    Helpers.Safe(() => Game.LocalPlayer.Money += 2000);
                }
            }

            PromoteGang(m.GangId);
            MaybeAssignNemesis();
        }

        void HandlePlayerDeath(Ped player)
        {
            Vector3 pp = new Vector3(0f, 0f, 0f);
            Helpers.Safe(() => pp = player.Position);

            GangMember killer = null;
            float best = 90f;
            foreach (KeyValuePair<string, Ped> kv in _memberPed)
            {
                GangMember m = _state.GetMember(kv.Key);
                if (m == null || !m.Alive || kv.Value == null) continue;
                float d = Helpers.Dist(pp, kv.Value.Position);
                if (d < best) { best = d; killer = m; }
            }

            if (killer != null)
            {
                killer.PlayerKills++;
                killer.GrudgeAgainstPlayer = true;
                if (killer.HasTrait(MemberTrait.Vindictive) || killer.PlayerKills >= 2)
                    killer.IsNemesis = true;
                Helpers.ShowMessage(killer.Name + " запомнил вас... (ваших смертей от него: " + killer.PlayerKills + ")", 5000);
            }
        }

        void PromoteGang(string gangId)
        {
            if (string.IsNullOrEmpty(gangId)) return;
            List<GangMember> gang = _state.MembersOfGang(gangId);
            GangMember top = null;
            foreach (GangMember m in gang)
                if (m.Alive && (top == null || m.Rank > top.Rank)) top = m;
            if (top != null && top.Rank < 10) { top.Rank++; top.Level++; }

            // recruit a replacement to keep the gang's numbers up
            Gang g = _state.GetGang(gangId);
            if (g != null)
            {
                GangMember recruit = new GangMember();
                recruit.Id = Guid.NewGuid().ToString("N");
                recruit.GangId = gangId;
                recruit.Name = NameGenerator.Generate(Helpers.Rng, null);
                recruit.Rank = 0;
                recruit.Level = 1;
                recruit.ModelName = "M_Y_GMODK_01";
                recruit.Alive = true;
                _state.Members.Add(recruit);
            }
        }

        void MaybeAssignNemesis()
        {
            if (_state.GetNemesis() != null) return;

            GangMember best = null;
            int bestScore = -1;
            foreach (GangMember m in _state.Members)
            {
                if (!m.Alive || m.IsNemesis) continue;
                int score = m.Rank * 3 + m.Level * 2 + m.PlayerKills * 5;
                if (m.HasTrait(MemberTrait.Vindictive)) score += 15;
                if (m.GrudgeAgainstPlayer) score += 10;
                if (score > bestScore) { bestScore = score; best = m; }
            }

            if (best != null && bestScore >= 12)
            {
                best.IsNemesis = true;
                best.Level += 2;
                Gang g = _state.GetGang(best.GangId);
                Helpers.ShowMessage("У вас появился НЕМЕЗИДА: " + best.Name + (g != null ? " (" + g.Name + ")" : ""), 7000);
            }
        }

        void MaybeSpawnEncounter()
        {
            Ped player = Helpers.Player;
            if (player == null || !player.isAlive) return;

            GangMember target = _state.GetNemesis();
            if (target == null)
            {
                // a grudge-holder or random enemy shows up
                List<GangMember> candidates = new List<GangMember>();
                foreach (GangMember m in _state.Members)
                    if (m.Alive && m.GrudgeAgainstPlayer) candidates.Add(m);
                if (candidates.Count == 0) return;
                target = candidates[Helpers.Rng.Next(candidates.Count)];
            }

            Vector3 pos = Helpers.RandomStreetNearPlayer(60f, 120f);
            Ped leader = SpawnMemberAt(target.Id, pos, true);
            if (leader == null) return;

            Helpers.ShowMessage("Вас выследил " + target.Name + "!", 5000);

            // bring bodyguards
            int guards = 0;
            foreach (GangMember g in _state.MembersOfGang(target.GangId))
            {
                if (guards >= 2) break;
                if (g.Id == target.Id || !g.Alive) continue;
                if (SpawnMemberAt(g.Id, Helpers.RandomStreetNearPlayer(60f, 120f), true) != null) guards++;
            }
        }
    }
}
