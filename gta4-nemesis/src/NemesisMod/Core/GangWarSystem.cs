using System;
using System.Collections.Generic;
using GTA;
using GTA4Nemesis.Model;

namespace GTA4Nemesis.Core
{
    /// <summary>
    /// Gang wars: territories change hands as the player (and rival gangs)
    /// fight over them. Rival gangs launch attacks the player must repel.
    /// </summary>
    public class GangWarSystem
    {
        readonly GameState _state;
        readonly Config _cfg;
        readonly NemesisSystem _nemesis;

        readonly Dictionary<string, int> _territoryScore = new Dictionary<string, int>(); // territory id -> control score
        readonly Dictionary<string, int> _attackKills = new Dictionary<string, int>();     // territory id -> attackers killed by player
        readonly Dictionary<string, DateTime> _attackDeadline = new Dictionary<string, DateTime>();
        readonly Dictionary<string, string> _attackerGang = new Dictionary<string, string>();

        DateTime _nextAttack = DateTime.MinValue;

        public GangWarSystem(GameState state, Config cfg, NemesisSystem nemesis)
        {
            _state = state;
            _cfg = cfg;
            _nemesis = nemesis;
        }

        public void Tick()
        {
            if (!_cfg.GangWarsEnabled) return;
            DateTime now = DateTime.UtcNow;

            if (now >= _nextAttack)
            {
                _nextAttack = now.AddSeconds(Helpers.Rng.Next(120, 300));
                MaybeLaunchAttack();
            }

            // resolve attack deadlines
            List<string> done = new List<string>();
            foreach (KeyValuePair<string, DateTime> kv in _attackDeadline)
            {
                if (now >= kv.Value)
                {
                    // attack timed out -> territory flips to the attacker
                    Territory t = FindTerritory(kv.Key);
                    string gangId;
                    if (t != null && _attackerGang.TryGetValue(kv.Key, out gangId))
                    {
                        t.ControllingGangId = gangId;
                        Helpers.ShowMessage("Банда " + GangName(gangId) + " захватила " + t.Name + "!", 5000);
                    }
                    done.Add(kv.Key);
                }
            }
            foreach (string id in done) _attackDeadline.Remove(id);
        }

        /// <summary>Player killed an enemy inside a territory.</summary>
        public void OnEnemyKilled(string memberId, Vector3 pos)
        {
            Territory t = TerritoryAt(pos);
            if (t == null) return;

            // attacking during an enemy assault
            if (_attackerGang.ContainsKey(t.Id))
            {
                int kills = _attackKills.ContainsKey(t.Id) ? _attackKills[t.Id] + 1 : 1;
                _attackKills[t.Id] = kills;
                if (kills >= 4)
                {
                    _attackDeadline.Remove(t.Id);
                    _attackerGang.Remove(t.Id);
                    _attackKills.Remove(t.Id);
                    Helpers.ShowMessage("Вы отразили атаку на " + t.Name + " (+$1500)", 5000);
                    Helpers.Safe(() => Game.LocalPlayer.Money += 1500);
                }
                return;
            }

            // capturing a territory from its gang
            int score = _territoryScore.ContainsKey(t.Id) ? _territoryScore[t.Id] : 10;
            score -= 2;
            if (score <= 0)
            {
                string old = t.ControllingGangId;
                t.ControllingGangId = "player";
                _territoryScore[t.Id] = 10;
                Helpers.ShowMessage("Территория захвачена: " + t.Name + " перешла под ваш контроль (+$2500)", 6000);
                Helpers.Safe(() => Game.LocalPlayer.Money += 2500);
            }
            else
            {
                _territoryScore[t.Id] = score;
            }
        }

        public Territory TerritoryAt(Vector3 pos)
        {
            Territory best = null;
            float bestD = float.MaxValue;
            foreach (Territory t in _state.Territories)
            {
                float d = Helpers.Dist(pos, new Vector3(t.X, t.Y, t.Z));
                if (d <= t.Radius && d < bestD) { bestD = d; best = t; }
            }
            return best;
        }

        public Territory FindTerritory(string id)
        {
            foreach (Territory t in _state.Territories) if (t.Id == id) return t;
            return null;
        }

        public string GangName(string gangId)
        {
            Gang g = _state.GetGang(gangId);
            return g != null ? g.Name : gangId;
        }

        public void MaybeLaunchAttack()
        {
            // pick a player-controlled or allied territory, send a rival gang to attack
            List<Territory> candidates = new List<Territory>();
            foreach (Territory t in _state.Territories)
                if (t.ControllingGangId == "player" || IsAlly(t.ControllingGangId))
                    candidates.Add(t);
            if (candidates.Count == 0) return;

            Territory target = candidates[Helpers.Rng.Next(candidates.Count)];
            string attackerId = PickEnemyGang();
            if (attackerId == null) return;

            Vector3 center = new Vector3(target.X, target.Y, target.Z);
            List<GangMember> attackers = _state.MembersOfGang(attackerId);
            if (attackers.Count == 0) return;

            int spawned = 0;
            foreach (GangMember m in attackers)
            {
                if (!m.Alive || spawned >= 4) continue;
                Vector3 pos = Helpers.SnapToStreet(new Vector3(
                    center.X + Helpers.Rng.Next(-120, 120),
                    center.Y + Helpers.Rng.Next(-120, 120),
                    center.Z));
                if (_nemesis.SpawnMemberAt(m.Id, pos, true) != null) spawned++;
            }

            _attackerGang[target.Id] = attackerId;
            _attackKills[target.Id] = 0;
            _attackDeadline[target.Id] = DateTime.UtcNow.AddSeconds(180);
            Helpers.ShowMessage("Тревога: " + GangName(attackerId) + " атакует " + target.Name + "! Уничтожьте нападавших.", 7000);
        }

        bool IsAlly(string gangId)
        {
            Gang g = _state.GetGang(gangId);
            return g != null && g.PlayerAlly;
        }

        string PickEnemyGang()
        {
            List<Gang> enemies = new List<Gang>();
            foreach (Gang g in _state.Gangs) if (!g.PlayerAlly) enemies.Add(g);
            if (enemies.Count == 0) return null;
            return enemies[Helpers.Rng.Next(enemies.Count)].Id;
        }
    }
}
