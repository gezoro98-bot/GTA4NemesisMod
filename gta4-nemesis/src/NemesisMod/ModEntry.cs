using System;
using System.Windows.Forms;
using GTA;
using GTA4Nemesis.Core;
using GTA4Nemesis.Model;

namespace GTA4Nemesis
{
    /// <summary>
    /// Script entry point. ScriptHookDotNet instantiates every Script-derived
    /// class in each *.net.dll found in the game's scripts folder.
    /// </summary>
    public class NemesisMod : Script
    {
        GameState _state;
        Config _cfg;
        NemesisSystem _nemesis;
        GangWarSystem _gangWar;
        ActivitySystem _activities;
        MissionSystem _missions;
        NpcBehavior _npc;

        DateTime _lastSave = DateTime.MinValue;

        public NemesisMod()
        {
            Interval = 500;
            Tick += OnTick;
            KeyDown += OnKeyDown;

            _cfg = Config.Load();
            _state = SaveStore.Load();
            WorldFactory.Seed(_state);

            _nemesis = new NemesisSystem(_state, _cfg);
            _gangWar = new GangWarSystem(_state, _cfg, _nemesis);
            _activities = new ActivitySystem(_state, _cfg, _nemesis);
            _missions = new MissionSystem(_state, _cfg, _nemesis, _gangWar);
            _npc = new NpcBehavior(_state, _cfg, _nemesis);

            Helpers.ShowMessage("GTA IV: Nemesis & Gang Wars загружен. "
                + "F9 — статус | F10 — атака банд | F11 — осн. задание | F12 — побочное | Num1/2 — контракты | Num3 — союзник",
                9000);
        }

        void OnTick(object sender, EventArgs e)
        {
            _nemesis.Tick();
            _gangWar.Tick();
            _activities.Tick();
            _missions.Tick();
            _npc.Tick();

            DateTime now = DateTime.UtcNow;
            if ((now - _lastSave).TotalSeconds >= _cfg.AutosaveSec)
            {
                _lastSave = now;
                SaveStore.Save(_state);
            }
        }

        void OnKeyDown(object sender, GTA.KeyEventArgs e)
        {
            if (e.Key == Keys.F9) ShowStatus();
            else if (e.Key == Keys.F10) _gangWar.MaybeLaunchAttack();
            else if (e.Key == Keys.F11) _missions.StartMainMission();
            else if (e.Key == Keys.F12) _missions.StartSideMission();
            else if (e.Key == Keys.NumPad1) _activities.OpenBounty();
            else if (e.Key == Keys.NumPad2) _activities.OpenAssassination();
            else if (e.Key == Keys.NumPad3) _npc.SpawnAlly();
        }

        void ShowStatus()
        {
            GangMember nemesis = _state.GetNemesis();
            int ours = 0;
            foreach (Territory t in _state.Territories)
                if (t.ControllingGangId == "player") ours++;

            int activeContracts = 0;
            foreach (Contract c in _state.Contracts) if (c.Active) activeContracts++;

            string line = "Немезида: " + (nemesis != null ? nemesis.Name : "нет")
                + " | Ваших территорий: " + ours + "/" + _state.Territories.Count
                + " | Контрактов: " + activeContracts
                + " | Убито врагов: " + _state.GetStatInt("nemesis_kills", 0);
            Helpers.ShowMessage(line, 6000);
            Helpers.Safe(() => Game.Console.Print(line));
            Helpers.Safe(() => Game.Console.Print(_missions.StatusLine()));
        }
    }
}
