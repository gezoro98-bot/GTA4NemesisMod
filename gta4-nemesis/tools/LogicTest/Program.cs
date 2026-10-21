using System;
using GTA4Nemesis.Core;
using GTA4Nemesis.Model;

class LogicTest
{
    static int _pass = 0, _fail = 0;

    static void Check(string name, bool ok)
    {
        if (ok) { _pass++; Console.WriteLine("PASS  " + name); }
        else { _fail++; Console.WriteLine("FAIL  " + name); }
    }

    static void Main()
    {
        // 1) NameGenerator
        string n1 = NameGenerator.Generate(new Random(1), null);
        string n2 = NameGenerator.Generate(new Random(2), null);
        Check("NameGenerator: non-empty", !string.IsNullOrEmpty(n1) && !string.IsNullOrEmpty(n2));
        Check("NameGenerator: different", n1 != n2);
        Check("NameGenerator: keeps existing", NameGenerator.Generate(new Random(1), "Existing") == "Existing");

        // 2) GangMember combat stats
        GangMember boss = new GangMember();
        boss.Rank = 10; boss.Level = 10;
        boss.Traits.Add(MemberTrait.Brute);
        boss.Traits.Add(MemberTrait.Marksman);
        Check("Boss MaxHealth > 150", boss.EffectiveMaxHealth() > 150);
        Check("Boss Accuracy capped at 100", boss.EffectiveAccuracy() == 100);
        Check("Boss CombatTier == 2", boss.CombatTier() == 2);
        GangMember weak = new GangMember();
        weak.Rank = 0; weak.Level = 1;
        Check("Weak CombatTier == 0", weak.CombatTier() == 0);

        // 3) SaveStore round-trip
        GameState s = new GameState();
        s.Gangs.Add(new Gang("rus", "Русская мафия", "NEM_RUS", false, 200, 30, 30));
        GangMember gm = new GangMember();
        gm.Id = "m1"; gm.GangId = "rus"; gm.Name = "Viktor Volkov"; gm.Rank = 8; gm.Level = 5;
        gm.Traits.Add(MemberTrait.Vindictive); gm.Alive = true; gm.IsNemesis = true;
        gm.ModelName = "M_Y_GRUS_HI_01"; gm.LastX = 1.5f; gm.LastY = -2.5f; gm.LastZ = 3.5f;
        s.Members.Add(gm);
        s.Territories.Add(new Territory("t1", "Бохан", 500f, 2200f, 10f, 300f));
        s.SetStatInt("nemesis_kills", 7);

        SaveStore.Save(s);
        GameState loaded = SaveStore.Load();

        Check("Gang round-trip", loaded.Gangs.Count == 1 && loaded.Gangs[0].Name == "Русская мафия");
        Check("Member count round-trip", loaded.Members.Count == 1);
        if (loaded.Members.Count == 1)
        {
            GangMember lm = loaded.Members[0];
            Check("Member fields round-trip", lm.Id == "m1" && lm.Name == "Viktor Volkov"
                && lm.Rank == 8 && lm.Level == 5 && lm.Alive && lm.IsNemesis
                && lm.ModelName == "M_Y_GRUS_HI_01");
            Check("Member trait round-trip", lm.HasTrait(MemberTrait.Vindictive));
            Check("Member floats round-trip",
                Math.Abs(lm.LastX - 1.5f) < 0.001f &&
                Math.Abs(lm.LastY + 2.5f) < 0.001f &&
                Math.Abs(lm.LastZ - 3.5f) < 0.001f);
        }
        Check("Territory round-trip", loaded.Territories.Count == 1 && loaded.Territories[0].Name == "Бохан");
        Check("Stat round-trip", loaded.GetStatInt("nemesis_kills", 0) == 7);
        Check("GetNemesis round-trip", loaded.GetNemesis() != null && loaded.GetNemesis().Id == "m1");
        Check("MembersOfGang", loaded.MembersOfGang("rus").Count == 1);
        Check("GetMember missing -> null", loaded.GetMember("zzz") == null);

        Console.WriteLine();
        Console.WriteLine("RESULT: " + _pass + " passed, " + _fail + " failed");
        Environment.Exit(_fail == 0 ? 0 : 1);
    }
}
