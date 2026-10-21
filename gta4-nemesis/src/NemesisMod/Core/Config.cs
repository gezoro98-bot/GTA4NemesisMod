using System;
using System.Collections.Generic;
using System.IO;

namespace GTA4Nemesis.Core
{
    /// <summary>
    /// Reads an optional scripts/NemesisMod.ini next to the mod DLL.
    /// Every key has a sensible default so the file is not required.
    /// </summary>
    public class Config
    {
        readonly Dictionary<string, string> _map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public bool NemesisEnabled = true;
        public bool GangWarsEnabled = true;
        public bool ActivitiesEnabled = true;
        public bool MissionsEnabled = true;
        public bool NpcAiEnabled = true;

        public int AmbushChancePct = 3;      // chance per check tick to trigger an ambush
        public int ShootoutChancePct = 5;    // chance to trigger a random shootout
        public int ActivityCooldownSec = 45; // min seconds between auto activities
        public int AutosaveSec = 60;

        public static Config Load()
        {
            Config c = new Config();
            string path = Path.Combine(SaveStore.GetDataDir(), "NemesisMod.ini");
            if (!File.Exists(path)) return c;
            try
            {
                foreach (string raw in File.ReadAllLines(path))
                {
                    string line = raw.Trim();
                    if (line.Length == 0 || line.StartsWith("#") || line.StartsWith(";")) continue;
                    int eq = line.IndexOf('=');
                    if (eq <= 0) continue;
                    c._map[line.Substring(0, eq).Trim()] = line.Substring(eq + 1).Trim();
                }
            }
            catch { }
            c.NemesisEnabled = c.Bool("nemesis", true);
            c.GangWarsEnabled = c.Bool("gangwars", true);
            c.ActivitiesEnabled = c.Bool("activities", true);
            c.MissionsEnabled = c.Bool("missions", true);
            c.NpcAiEnabled = c.Bool("npcai", true);
            c.AmbushChancePct = c.Int("ambush_chance", 3);
            c.ShootoutChancePct = c.Int("shootout_chance", 5);
            c.ActivityCooldownSec = c.Int("activity_cooldown_sec", 45);
            c.AutosaveSec = c.Int("autosave_sec", 60);
            return c;
        }

        bool Bool(string k, bool d) { string v; return _map.TryGetValue(k, out v) ? (v == "1" || v.Equals("true", StringComparison.OrdinalIgnoreCase)) : d; }
        int Int(string k, int d) { string v; int r; return _map.TryGetValue(k, out v) && int.TryParse(v, out r) ? r : d; }
    }
}
