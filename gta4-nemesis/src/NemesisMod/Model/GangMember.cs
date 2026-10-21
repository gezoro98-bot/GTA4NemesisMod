using System.Collections.Generic;

namespace GTA4Nemesis.Model
{
    public enum MemberTrait
    {
        Leader,      // boosts nearby allies, higher rank
        Marksman,    // higher accuracy
        Brute,       // more health/armor
        Fast,        // moves faster, harder to pin down
        Coward,      // flees when hurt
        Ambusher,    // spawns in ambushes more often
        Loyal,       // never betrays; avenges fallen allies
        Disloyal,    // may switch sides if paid
        Fearless,    // never flees
        Vindictive   // becomes Nemesis after surviving the player
    }

    /// <summary>
    /// A named, persistent gang member — the heart of the Nemesis system.
    /// </summary>
    public class GangMember
    {
        public string Id;
        public string GangId;
        public string Name;
        public int Rank;          // 0 = foot soldier .. 10 = boss
        public int Level;         // grows as they survive / win
        public List<MemberTrait> Traits = new List<MemberTrait>();
        public bool Alive = true;
        public bool GrudgeAgainstPlayer;   // hates the player personally
        public int PlayerKills;            // times they killed the player
        public bool IsNemesis;             // promoted to the player's nemesis
        public string ModelName;           // ped model, e.g. "M_Y_GMAF_HI_01"
        public float LastX, LastY, LastZ;  // last known position (for respawns)

        public GangMember() { }

        public bool HasTrait(MemberTrait t) { return Traits.Contains(t); }

        /// <summary>Effective combat tier 0..2 used to pick weapons/health.</summary>
        public int CombatTier()
        {
            int tier = Rank >= 7 ? 2 : (Rank >= 3 ? 1 : 0);
            if (HasTrait(MemberTrait.Brute)) tier = tier < 2 ? tier + 1 : tier;
            if (Level >= 5) tier = tier < 2 ? tier + 1 : tier;
            return tier;
        }

        /// <summary>Effective accuracy 20..100.</summary>
        public int EffectiveAccuracy()
        {
            int a = 40 + Rank * 4 + Level * 2;
            if (HasTrait(MemberTrait.Marksman)) a += 25;
            if (a > 100) a = 100;
            if (a < 20) a = 20;
            return a;
        }

        /// <summary>Effective max health 100..300.</summary>
        public int EffectiveMaxHealth()
        {
            int h = 100 + Rank * 8 + Level * 5;
            if (HasTrait(MemberTrait.Brute)) h += 60;
            if (h > 300) h = 300;
            return h;
        }
    }
}
