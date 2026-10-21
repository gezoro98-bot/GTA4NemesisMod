using System.Collections.Generic;

namespace GTA4Nemesis.Model
{
    /// <summary>
    /// The entire persistent world state, serialized to disk by SaveStore.
    /// </summary>
    public class GameState
    {
        public const int SaveVersion = 1;

        public List<Gang> Gangs = new List<Gang>();
        public List<GangMember> Members = new List<GangMember>();
        public List<Territory> Territories = new List<Territory>();
        public List<Contract> Contracts = new List<Contract>();
        public Dictionary<string, string> Stats = new Dictionary<string, string>();

        public Gang GetGang(string id)
        {
            foreach (Gang g in Gangs) if (g.Id == id) return g;
            return null;
        }

        public GangMember GetMember(string id)
        {
            foreach (GangMember m in Members) if (m.Id == id) return m;
            return null;
        }

        public List<GangMember> MembersOfGang(string gangId)
        {
            List<GangMember> list = new List<GangMember>();
            foreach (GangMember m in Members) if (m.GangId == gangId) list.Add(m);
            return list;
        }

        public GangMember GetNemesis()
        {
            foreach (GangMember m in Members) if (m.IsNemesis && m.Alive) return m;
            return null;
        }

        public string GetStat(string key, string fallback)
        {
            string v;
            return Stats.TryGetValue(key, out v) ? v : fallback;
        }

        public void SetStat(string key, string value)
        {
            Stats[key] = value;
        }

        public int GetStatInt(string key, int fallback)
        {
            int v;
            return int.TryParse(GetStat(key, null), out v) ? v : fallback;
        }

        public void SetStatInt(string key, int value)
        {
            SetStat(key, value.ToString());
        }
    }
}
