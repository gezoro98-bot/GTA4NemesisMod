using System;
using System.Collections.Generic;
using GTA;
using GTA4Nemesis.Model;

namespace GTA4Nemesis.Core
{
    /// <summary>
    /// "AI improvement" layer. GTA IV's pathfinding cannot be rewritten from
    /// script, but we can push combat stats and behaviour far beyond default:
    /// accuracy, health, aggression, and allies that actually fight instead of
    /// fleeing.
    /// </summary>
    public class NpcBehavior
    {
        readonly Config _cfg;
        readonly List<Ped> _allies = new List<Ped>();

        public NpcBehavior(GameState state, Config cfg, NemesisSystem nemesis)
        {
            _cfg = cfg;
        }

        /// <summary>Spawn an allied bodyguard that fights for the player.</summary>
        public void SpawnAlly()
        {
            Ped player = Helpers.Player;
            if (player == null) return;

            Vector3 pos = Helpers.RandomStreetNearPlayer(10f, 30f);
            Ped ally = Helpers.SpawnPed("M_Y_GMODK_01", pos);
            if (ally == null) return;

            ApplyCombatProfile(ally, 60, 250, 1);
            Helpers.SetFriendlyToPlayer(ally);
            Helpers.Safe(() => ally.Task.AlwaysKeepTask = true);
            Helpers.Safe(() => ally.WantedByPolice = false);
            _allies.Add(ally);
            Helpers.ShowMessage("Союзник присоединился к вам.", 3000);
        }

        public void Tick()
        {
            if (!_cfg.NpcAiEnabled) return;

            List<Ped> remove = new List<Ped>();
            foreach (Ped ally in _allies)
            {
                if (ally == null || !ally.isAlive) { remove.Add(ally); continue; }
                // keep allies fighting any hated target nearby
                Helpers.Safe(() => { if (!ally.isInCombat) ally.Task.FightAgainstHatedTargets(120f); });
            }
            foreach (Ped a in remove) _allies.Remove(a);
        }

        /// <summary>Apply combat buffs to any mod-spawned ped.</summary>
        public void ApplyCombatProfile(Ped p, int accuracy, int maxHealth, int tier)
        {
            Helpers.Safe(() => p.Accuracy = accuracy);
            Helpers.SetHealth(p, maxHealth);
            Helpers.ArmPed(p, tier);
            Helpers.Safe(() => p.Task.FightAgainstHatedTargets(100f));
        }
    }
}
