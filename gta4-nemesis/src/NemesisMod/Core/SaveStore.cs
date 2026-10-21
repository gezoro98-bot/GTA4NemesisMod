using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using GTA4Nemesis.Model;

namespace GTA4Nemesis.Core
{
    /// <summary>
    /// Line-based persistence for GameState. Avoids external JSON libraries
    /// and works on .NET Framework 4.0 out of the box.
    /// </summary>
    public static class SaveStore
    {
        const string Header = "NEMESIS-SAVE";
        const string FileName = "state.txt";

        public static string GetDataDir()
        {
            try
            {
                string code = typeof(SaveStore).Assembly.CodeBase;
                if (!string.IsNullOrEmpty(code))
                {
                    string dir = Path.GetDirectoryName(new Uri(code).LocalPath);
                    if (!string.IsNullOrEmpty(dir)) return dir;
                }
            }
            catch { }
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "GTA4Nemesis");
        }

        public static string GetSavePath()
        {
            return Path.Combine(GetDataDir(), FileName);
        }

        public static GameState Load()
        {
            GameState s = new GameState();
            string path = GetSavePath();
            if (!File.Exists(path)) return s;

            try
            {
                foreach (string raw in File.ReadAllLines(path))
                {
                    string line = raw.Trim();
                    if (line.Length == 0 || line.StartsWith("#")) continue;
                    string[] f = line.Split('|');
                    if (f.Length == 0) continue;

                    switch (f[0])
                    {
                        case "GANG":
                            if (f.Length >= 8)
                                s.Gangs.Add(new Gang(f[1], f[2], f[3], f[4] == "1",
                                    I(f[5]), I(f[6]), I(f[7])));
                            break;
                        case "MEMBER":
                            s.Members.Add(ParseMember(f));
                            break;
                        case "TERRITORY":
                            if (f.Length >= 7)
                                s.Territories.Add(new Territory(f[1], f[2],
                                    F(f[3]), F(f[4]), F(f[5]), F(f[6])));
                            break;
                        case "CONTRACT":
                            if (f.Length >= 8)
                            {
                                Contract c = new Contract();
                                c.Id = f[1];
                                c.Type = f[2] == "A" ? ContractType.Assassination : ContractType.Bounty;
                                c.TargetMemberId = f[3];
                                c.TargetLabel = f[4];
                                c.Reward = I(f[5]);
                                c.Active = f[6] == "1";
                                c.X = F(f[7]); c.Y = F(f[8]); c.Z = F(f[9]);
                                s.Contracts.Add(c);
                            }
                            break;
                        case "STAT":
                            if (f.Length >= 3) s.Stats[f[1]] = f[2];
                            break;
                    }
                }
            }
            catch { }

            return s;
        }

        public static void Save(GameState s)
        {
            try
            {
                string dir = GetDataDir();
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

                List<string> lines = new List<string>();
                lines.Add("# " + Header + " v" + GameState.SaveVersion);

                foreach (Gang g in s.Gangs)
                    lines.Add(string.Join("|", "GANG", g.Id, g.Name, g.RelationshipGroupName,
                        g.PlayerAlly ? "1" : "0", g.ColorR, g.ColorG, g.ColorB));

                foreach (GangMember m in s.Members)
                    lines.Add(SerializeMember(m));

                foreach (Territory t in s.Territories)
                    lines.Add(string.Join("|", "TERRITORY", t.Id, t.Name,
                        t.ControllingGangId ?? "", F(t.X), F(t.Y), F(t.Z), F(t.Radius)));

                foreach (Contract c in s.Contracts)
                    lines.Add(string.Join("|", "CONTRACT", c.Id,
                        c.Type == ContractType.Assassination ? "A" : "B",
                        c.TargetMemberId ?? "", c.TargetLabel ?? "", c.Reward,
                        c.Active ? "1" : "0", F(c.X), F(c.Y), F(c.Z)));

                foreach (KeyValuePair<string, string> kv in s.Stats)
                    lines.Add(string.Join("|", "STAT", kv.Key, kv.Value));

                File.WriteAllLines(GetSavePath(), lines.ToArray());
            }
            catch { }
        }

        static GangMember ParseMember(string[] f)
        {
            GangMember m = new GangMember();
            if (f.Length >= 2) m.Id = f[1];
            if (f.Length >= 3) m.GangId = f[2];
            if (f.Length >= 4) m.Name = f[3];
            if (f.Length >= 5) m.Rank = I(f[4]);
            if (f.Length >= 6) m.Level = I(f[5]);
            if (f.Length >= 7)
            {
                foreach (string t in f[6].Split(','))
                {
                    MemberTrait mt;
                    if (Enum.TryParse(t.Trim(), out mt)) m.Traits.Add(mt);
                }
            }
            if (f.Length >= 8) m.Alive = f[7] == "1";
            if (f.Length >= 9) m.GrudgeAgainstPlayer = f[8] == "1";
            if (f.Length >= 10) m.PlayerKills = I(f[9]);
            if (f.Length >= 11) m.IsNemesis = f[10] == "1";
            if (f.Length >= 12) m.ModelName = f[11];
            if (f.Length >= 15) { m.LastX = F(f[12]); m.LastY = F(f[13]); m.LastZ = F(f[14]); }
            return m;
        }

        static string SerializeMember(GangMember m)
        {
            List<string> traits = new List<string>();
            foreach (MemberTrait t in m.Traits) traits.Add(t.ToString());
            return string.Join("|", "MEMBER", m.Id, m.GangId, m.Name, m.Rank, m.Level,
                string.Join(",", traits.ToArray()), m.Alive ? "1" : "0",
                m.GrudgeAgainstPlayer ? "1" : "0", m.PlayerKills, m.IsNemesis ? "1" : "0",
                m.ModelName ?? "", F(m.LastX), F(m.LastY), F(m.LastZ));
        }

        static int I(string s) { int v; return int.TryParse(s, out v) ? v : 0; }
        static float F(string s) { float v; return float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out v) ? v : 0f; }
        static string F(float v) { return v.ToString("0.##", CultureInfo.InvariantCulture); }
    }
}
