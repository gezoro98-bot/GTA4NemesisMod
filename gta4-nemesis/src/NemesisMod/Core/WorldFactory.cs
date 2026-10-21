using System;
using System.Collections.Generic;
using GTA4Nemesis.Core;
using GTA4Nemesis.Model;

namespace GTA4Nemesis.Core
{
    /// <summary>
    /// Seeds the initial persistent world (gangs, members, territories) when
    /// no save exists yet.
    /// </summary>
    public static class WorldFactory
    {
        public static void Seed(GameState s)
        {
            if (s.Gangs.Count > 0) return;

            Random rng = Helpers.Rng;

            s.Gangs.Add(new Gang("rus", "Русская мафия", "NEM_RUS", false, 200, 30, 30));
            s.Gangs.Add(new Gang("tri", "Триады", "NEM_TRI", false, 200, 160, 30));
            s.Gangs.Add(new Gang("maf", "Итальянская мафия", "NEM_MAF", false, 60, 90, 200));
            s.Gangs.Add(new Gang("bik", "Байкеры", "NEM_BIK", false, 40, 180, 120));
            s.Gangs.Add(new Gang("jam", "Ямайцы", "NEM_JAM", false, 30, 180, 30));

            foreach (Gang g in s.Gangs)
            {
                List<string> models = ModelsFor(g.Id);
                // boss
                AddMember(s, g, rng, models, 8 + rng.Next(3), true);
                // lieutenants
                int lieut = 2 + rng.Next(2);
                for (int i = 0; i < lieut; i++) AddMember(s, g, rng, models, 4 + rng.Next(4), false);
                // soldiers
                int sol = 4 + rng.Next(3);
                for (int i = 0; i < sol; i++) AddMember(s, g, rng, models, rng.Next(4), false);
            }

            // Territories (approximate Liberty City centers; snapped to street at spawn).
            s.Territories.Add(new Territory("t_bohan", "Бохан", 500f, 2200f, 10f, 300f));
            s.Territories.Add(new Territory("t_northwood", "Нортвуд", 0f, 1600f, 10f, 300f));
            s.Territories.Add(new Territory("t_starj", "Стар-Джанкшен", -200f, 600f, 10f, 300f));
            s.Territories.Add(new Territory("t_park", "Миддл-Парк", 400f, 900f, 10f, 300f));
            s.Territories.Add(new Territory("t_alderney", "Олдерни", -1400f, -200f, 10f, 300f));
            s.Territories.Add(new Territory("t_hove", "Хоув-Бич", 1600f, -1400f, 10f, 300f));
            s.Territories.Add(new Territory("t_schottler", "Шоттлер", 1700f, -700f, 10f, 300f));
            s.Territories.Add(new Territory("t_dukes", "Дюкс", 1200f, 800f, 10f, 300f));
            s.Territories.Add(new Territory("t_firefly", "Файрфлай-Айленд", -300f, -1700f, 10f, 300f));
            s.Territories.Add(new Territory("t_docks", "Доки Брокера", 1900f, -1100f, 10f, 300f));

            for (int i = 0; i < s.Territories.Count; i++)
                s.Territories[i].ControllingGangId = s.Gangs[i % s.Gangs.Count].Id;

            s.SetStatInt("nemesis_kills", 0);
        }

        static void AddMember(GameState s, Gang g, Random rng, List<string> models, int rank, bool boss)
        {
            GangMember m = new GangMember();
            m.Id = Guid.NewGuid().ToString("N");
            m.GangId = g.Id;
            m.Name = NameGenerator.Generate(rng, null);
            m.Rank = rank;
            m.Level = 1 + rng.Next(3);
            m.ModelName = models[rng.Next(models.Count)];
            m.Alive = true;
            m.Traits.Add(boss ? MemberTrait.Leader : RandomTrait(rng));
            if (rng.Next(4) == 0) m.Traits.Add(RandomTrait(rng));
            s.Members.Add(m);
        }

        static MemberTrait RandomTrait(Random rng)
        {
            Array vals = Enum.GetValues(typeof(MemberTrait));
            return (MemberTrait)vals.GetValue(rng.Next(vals.Length));
        }

        static List<string> ModelsFor(string gangId)
        {
            List<string> m = new List<string>();
            switch (gangId)
            {
                case "rus": m.Add("M_Y_GRUS_HI_01"); m.Add("M_Y_GRUS_LO_01"); break;
                case "tri": m.Add("M_Y_GTRI_01"); m.Add("M_Y_GTRI_02"); break;
                case "maf": m.Add("M_Y_GMAF_HI_01"); m.Add("M_Y_GMAF_LO_01"); break;
                case "bik": m.Add("M_Y_GBIK_HI_01"); m.Add("M_Y_GBIK_LO_01"); break;
                case "jam": m.Add("M_Y_GJAM_HI_01"); m.Add("M_Y_GJAM_LO_01"); break;
                default: m.Add("M_Y_GMODK_01"); break;
            }
            return m;
        }
    }
}
