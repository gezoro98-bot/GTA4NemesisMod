using System;
using System.Collections.Generic;
using GTA;
using GTA4Nemesis.Model;

namespace GTA4Nemesis.Core
{
    public enum MissionKind
    {
        None,
        HuntNemesis,     // main: locate and kill the current nemesis
        DefendTerritory, // side: repel an attack on a territory
        EliminateTarget  // side: kill a named target
    }

    /// <summary>
    /// Lightweight mission engine: a chain of objective stages with blips,
    /// completion detection and rewards. Full cutscenes/voice are out of scope.
    /// </summary>
    public class MissionSystem
    {
        readonly GameState _state;
        readonly Config _cfg;
        readonly NemesisSystem _nemesis;
        readonly GangWarSystem _gangWar;

        MissionKind _active = MissionKind.None;
        string _targetMemberId;
        string _targetTerritoryId;
        Blip _objectiveBlip;
        DateTime _started;

        public MissionSystem(GameState state, Config cfg, NemesisSystem nemesis, GangWarSystem gangWar)
        {
            _state = state;
            _cfg = cfg;
            _nemesis = nemesis;
            _gangWar = gangWar;
        }

        public bool HasActive { get { return _active != MissionKind.None; } }

        public void StartMainMission()
        {
            if (HasActive) { Helpers.ShowMessage("Сначала завершите текущее задание.", 3000); return; }
            GangMember nemesis = _state.GetNemesis();
            if (nemesis == null)
            {
                Helpers.ShowMessage("Сейчас нет активной Немезиды. Устраняйте членов банд, и она появится.", 5000);
                return;
            }
            _active = MissionKind.HuntNemesis;
            _targetMemberId = nemesis.Id;
            _started = DateTime.UtcNow;
            Helpers.ShowMessage("ЗАДАНИЕ: охота на Немезиду — " + nemesis.Name, 6000);
            SpawnTargetAtRandomStreet(nemesis.Id);
        }

        public void StartSideMission()
        {
            if (HasActive) { Helpers.ShowMessage("Сначала завершите текущее задание.", 3000); return; }
            // 50/50 defend territory vs eliminate target
            if (Helpers.Rng.Next(2) == 0) StartDefendTerritory();
            else StartEliminateTarget();
        }

        void StartDefendTerritory()
        {
            List<Territory> ours = new List<Territory>();
            foreach (Territory x in _state.Territories)
                if (x.ControllingGangId == "player") ours.Add(x);
            if (ours.Count == 0)
            {
                StartEliminateTarget();
                return;
            }
            Territory t = ours[Helpers.Rng.Next(ours.Count)];
            _active = MissionKind.DefendTerritory;
            _targetTerritoryId = t.Id;
            _started = DateTime.UtcNow;
            Helpers.ShowMessage("ЗАДАНИЕ: удержите территорию " + t.Name + " — отбейте атаку.", 6000);

            Vector3 c = new Vector3(t.X, t.Y, t.Z);
            Blip b = Helpers.CreateBlip(c, BlipColor.Yellow, false);
            _objectiveBlip = b;

            // spawn attackers
            string attacker = PickEnemyGang();
            if (attacker != null)
            {
                int n = 0;
                foreach (GangMember m in _state.MembersOfGang(attacker))
                {
                    if (!m.Alive || n >= 3) continue;
                    Vector3 pos = Helpers.SnapToStreet(new Vector3(c.X + Helpers.Rng.Next(-100, 100), c.Y + Helpers.Rng.Next(-100, 100), c.Z));
                    if (_nemesis.SpawnMemberAt(m.Id, pos, true) != null) n++;
                }
            }
        }

        void StartEliminateTarget()
        {
            List<GangMember> pool = new List<GangMember>();
            foreach (GangMember m in _state.Members) if (m.Alive && m.Rank >= 2) pool.Add(m);
            if (pool.Count == 0) { Helpers.ShowMessage("Нет доступных целей.", 3000); return; }
            GangMember t = pool[Helpers.Rng.Next(pool.Count)];
            _active = MissionKind.EliminateTarget;
            _targetMemberId = t.Id;
            _started = DateTime.UtcNow;
            Helpers.ShowMessage("ЗАДАНИЕ: устраните " + t.Name + " (" + _state.GetGang(t.GangId).Name + ")", 6000);
            SpawnTargetAtRandomStreet(t.Id);
        }

        void SpawnTargetAtRandomStreet(string memberId)
        {
            Vector3 pos = Helpers.RandomStreetNearPlayer(120f, 400f);
            _nemesis.SpawnMemberAt(memberId, pos, true);
            Blip b = Helpers.CreateBlip(pos, BlipColor.Red, false);
            _objectiveBlip = b;
        }

        public void Tick()
        {
            if (!_cfg.MissionsEnabled || !HasActive) return;

            switch (_active)
            {
                case MissionKind.HuntNemesis:
                    UpdateKillTarget("Немезида", 4000);
                    break;
                case MissionKind.EliminateTarget:
                    UpdateKillTarget("Цель", 1200);
                    break;
                case MissionKind.DefendTerritory:
                    UpdateDefend();
                    break;
            }
        }

        void UpdateKillTarget(string label, int reward)
        {
            GangMember m = _state.GetMember(_targetMemberId);
            if (m == null || !m.Alive)
            {
                Helpers.Safe(() => Game.LocalPlayer.Money += reward);
                Helpers.ShowMessage("Задание выполнено: " + label + " устранён (+$" + reward + ")", 6000);
                Complete();
                return;
            }
            // keep blip tracking
            Ped p = _nemesis.GetLivePed(_targetMemberId);
            if (p == null && _objectiveBlip == null)
                SpawnTargetAtRandomStreet(_targetMemberId);
        }

        void UpdateDefend()
        {
            Territory t = _gangWar.FindTerritory(_targetTerritoryId);
            if (t == null) { Complete(); return; }

            // count living enemy members nearby
            int hostiles = 0;
            Vector3 c = new Vector3(t.X, t.Y, t.Z);
            foreach (GangMember m in _state.Members)
            {
                if (!m.Alive) continue;
                if (m.GangId == t.ControllingGangId || _state.GetGang(m.GangId) == null || _state.GetGang(m.GangId).PlayerAlly) continue;
                Ped p = _nemesis.GetLivePed(m.Id);
                if (p != null && p.isAlive && Helpers.Near(p.Position, c, t.Radius)) hostiles++;
            }

            if (hostiles == 0 && (DateTime.UtcNow - _started).TotalSeconds > 20)
            {
                Helpers.Safe(() => Game.LocalPlayer.Money += 1500);
                Helpers.ShowMessage("Задание выполнено: территория удержана (+$1500)", 6000);
                Complete();
            }
        }

        void Complete()
        {
            _active = MissionKind.None;
            _targetMemberId = null;
            _targetTerritoryId = null;
            if (_objectiveBlip != null) { Helpers.Safe(() => _objectiveBlip.Delete()); _objectiveBlip = null; }
        }

        public string StatusLine()
        {
            switch (_active)
            {
                case MissionKind.HuntNemesis: return "Задание: охота на Немезиду (" + NameOf(_targetMemberId) + ")";
                case MissionKind.DefendTerritory: return "Задание: оборона территории";
                case MissionKind.EliminateTarget: return "Задание: устранение цели (" + NameOf(_targetMemberId) + ")";
                default: return "Активных заданий нет";
            }
        }

        string NameOf(string memberId)
        {
            GangMember m = _state.GetMember(memberId);
            return m != null ? m.Name : "?";
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
