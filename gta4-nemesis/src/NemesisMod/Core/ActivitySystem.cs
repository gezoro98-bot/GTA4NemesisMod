using System;
using System.Collections.Generic;
using GTA;
using GTA4Nemesis.Model;

namespace GTA4Nemesis.Core
{
    /// <summary>
    /// Extra activities: bounties, assassination contracts, random shootouts
    /// and gang ambushes.
    /// </summary>
    public class ActivitySystem
    {
        readonly GameState _state;
        readonly Config _cfg;
        readonly NemesisSystem _nemesis;

        DateTime _nextRandom = DateTime.MinValue;
        readonly Dictionary<string, Blip> _contractBlips = new Dictionary<string, Blip>();

        public ActivitySystem(GameState state, Config cfg, NemesisSystem nemesis)
        {
            _state = state;
            _cfg = cfg;
            _nemesis = nemesis;
        }

        public void Tick()
        {
            if (!_cfg.ActivitiesEnabled) return;
            DateTime now = DateTime.UtcNow;

            if (now >= _nextRandom)
            {
                _nextRandom = now.AddSeconds(_cfg.ActivityCooldownSec);
                RollRandomActivity();
            }

            UpdateContracts();
        }

        /// <summary>Player action: open a new bounty contract.</summary>
        public void OpenBounty()
        {
            GenerateContract(ContractType.Bounty);
        }

        /// <summary>Player action: open a new assassination contract.</summary>
        public void OpenAssassination()
        {
            GenerateContract(ContractType.Assassination);
        }

        void GenerateContract(ContractType type)
        {
            // one active contract at a time (per type) for simplicity
            foreach (Contract existing in _state.Contracts)
                if (existing.Active && existing.Type == type) { Helpers.ShowMessage("У вас уже есть активный контракт этого типа.", 3000); return; }

            List<GangMember> pool = new List<GangMember>();
            foreach (GangMember m in _state.Members)
            {
                if (!m.Alive) continue;
                if (type == ContractType.Assassination && m.Rank < 4) continue;
                pool.Add(m);
            }
            if (pool.Count == 0) { Helpers.ShowMessage("Сейчас нет доступных целей.", 3000); return; }

            GangMember target = pool[Helpers.Rng.Next(pool.Count)];
            Vector3 pos = Helpers.RandomStreetNearPlayer(120f, 400f);

            Contract c = new Contract();
            c.Id = Guid.NewGuid().ToString("N");
            c.Type = type;
            c.TargetMemberId = target.Id;
            c.TargetLabel = target.Name;
            c.Reward = type == ContractType.Assassination
                ? 1500 + target.Rank * 200 + target.Level * 40
                : 400 + target.Rank * 80 + target.Level * 20;
            c.Active = true;
            c.X = pos.X; c.Y = pos.Y; c.Z = pos.Z;
            _state.Contracts.Add(c);

            SpawnContractTarget(c);

            string what = type == ContractType.Assassination ? "Заказ на устранение" : "Розыск";
            Helpers.ShowMessage(what + ": цель — " + target.Name + ". Награда $" + c.Reward, 7000);
        }

        void SpawnContractTarget(Contract c)
        {
            Vector3 pos = new Vector3(c.X, c.Y, c.Z);
            Ped p = _nemesis.SpawnMemberAt(c.TargetMemberId, pos, true);
            Blip b = Helpers.CreateBlip(pos, c.Type == ContractType.Assassination ? BlipColor.Red : BlipColor.Orange, false);
            if (b != null) _contractBlips[c.Id] = b;
        }

        void UpdateContracts()
        {
            List<Contract> done = new List<Contract>();
            foreach (Contract c in _state.Contracts)
            {
                if (!c.Active) continue;
                GangMember m = _state.GetMember(c.TargetMemberId);
                if (m == null || !m.Alive)
                {
                    Helpers.Safe(() => Game.LocalPlayer.Money += c.Reward);
                    Helpers.ShowMessage("Контракт выполнен: " + c.TargetLabel + " устранён (+$" + c.Reward + ")", 6000);
                    c.Active = false;
                    Blip b;
                    if (_contractBlips.TryGetValue(c.Id, out b)) { Helpers.Safe(() => b.Delete()); _contractBlips.Remove(c.Id); }
                    done.Add(c);
                    continue;
                }

                // refresh blip to the live ped position
                Ped p = _nemesis.GetLivePed(c.TargetMemberId);
                if (p != null) { c.X = p.Position.X; c.Y = p.Position.Y; c.Z = p.Position.Z; }
            }
            foreach (Contract c in done) _state.Contracts.Remove(c);
        }

        void RollRandomActivity()
        {
            Ped player = Helpers.Player;
            if (player == null || !player.isAlive) return;

            int roll = Helpers.Rng.Next(100);
            if (roll < _cfg.AmbushChancePct) SpawnAmbush();
            else if (roll < _cfg.AmbushChancePct + _cfg.ShootoutChancePct) SpawnShootout();
        }

        void SpawnAmbush()
        {
            // a grudge-holder or random enemy squad ambushes the player
            List<GangMember> enemies = new List<GangMember>();
            foreach (GangMember m in _state.Members)
                if (m.Alive && (m.GrudgeAgainstPlayer || m.IsNemesis)) enemies.Add(m);
            if (enemies.Count == 0)
            {
                foreach (GangMember m in _state.Members) if (m.Alive) enemies.Add(m);
                if (enemies.Count == 0) return;
            }

            GangMember leader = enemies[Helpers.Rng.Next(enemies.Count)];
            Vector3 pos = Helpers.RandomStreetNearPlayer(30f, 70f);
            _nemesis.SpawnMemberAt(leader.Id, pos, true);
            // bring two more from same gang
            int extra = 0;
            foreach (GangMember m in _state.MembersOfGang(leader.GangId))
            {
                if (extra >= 2) break;
                if (m.Id == leader.Id || !m.Alive) continue;
                if (_nemesis.SpawnMemberAt(m.Id, Helpers.RandomStreetNearPlayer(30f, 70f), true) != null) extra++;
            }
            Helpers.ShowMessage("Засада! " + leader.Name + " устроил вам ловушку.", 5000);
        }

        void SpawnShootout()
        {
            // two rival squads clash near the player (player is not involved)
            List<Gang> gangs = new List<Gang>();
            foreach (Gang g in _state.Gangs) if (!g.PlayerAlly) gangs.Add(g);
            if (gangs.Count < 2) return;

            Gang a = gangs[Helpers.Rng.Next(gangs.Count)];
            Gang b;
            do { b = gangs[Helpers.Rng.Next(gangs.Count)]; } while (b.Id == a.Id);

            Vector3 center = Helpers.RandomStreetNearPlayer(80f, 200f);
            SpawnSquad(a.Id, center, 2, b.Id);
            SpawnSquad(b.Id, center, 2, a.Id);
            Helpers.ShowMessage("Перестрелка: " + a.Name + " против " + b.Name + " поблизости.", 5000);
        }

        void SpawnSquad(string gangId, Vector3 center, int count, string enemyGangId)
        {
            List<GangMember> members = _state.MembersOfGang(gangId);
            int spawned = 0;
            foreach (GangMember m in members)
            {
                if (spawned >= count) break;
                if (!m.Alive) continue;
                Vector3 pos = Helpers.SnapToStreet(new Vector3(
                    center.X + Helpers.Rng.Next(-60, 60),
                    center.Y + Helpers.Rng.Next(-60, 60),
                    center.Z));
                Ped p = _nemesis.SpawnMemberAt(m.Id, pos, true);
                if (p != null)
                {
                    // direct them at the rival squad's members
                    Ped target = FindNearestLiveMember(enemyGangId, pos);
                    if (target != null) Helpers.Safe(() => p.Task.FightAgainst(target));
                    spawned++;
                }
            }
        }

        Ped FindNearestLiveMember(string gangId, Vector3 pos)
        {
            Ped best = null;
            float bestD = float.MaxValue;
            foreach (GangMember m in _state.MembersOfGang(gangId))
            {
                if (!m.Alive) continue;
                Ped p = _nemesis.GetLivePed(m.Id);
                if (p == null || !p.isAlive) continue;
                float d = Helpers.Dist(pos, p.Position);
                if (d < bestD) { bestD = d; best = p; }
            }
            return best;
        }
    }
}
