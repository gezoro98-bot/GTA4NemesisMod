// Compile-check stub of the ScriptHookDotNet v1.7.1.8 GTA API surface used by
// this mod. NOT for distribution — it exists only to syntax-check the mod
// without the real DLL. Signatures mirror the real API (verified from source).
using System;
using System.Windows.Forms;

namespace GTA
{
    public struct Vector3
    {
        public float X, Y, Z;
        public Vector3(float x, float y, float z) { X = x; Y = y; Z = z; }
    }

    public struct Model
    {
        public int Hash;
        public Model(int hash) { Hash = hash; }
        public static implicit operator Model(string name) { return new Model(name != null ? name.GetHashCode() : 0); }
    }

    public enum Weapon { None = -1, Unarmed = 0, Handgun_Glock = 7, Handgun_DesertEagle = 9, SMG_Uzi = 12, SMG_MP5 = 13, Rifle_AK47 = 14, Rifle_M4 = 15 }

    public enum Relationship { Companion = 0, Respect = 1, Like = 2, Neutral = 3, Dislike = 4, Hate = 5 }

    public enum RelationshipGroup { Player, Civillian_Male, Cop, Gang_Russian1, Gang_Russian2, Gang_Italian, Gang_Irish, Gang_Jamaican, Gang_Biker1, Gang_Biker2, Gang_ChineseJapanese, Criminal }

    public enum BlipColor { White = 0, Yellow = 5, Orange = 6, Red = 9 }

    public class Blip
    {
        public BlipColor Color { get; set; }
        public bool Friendly { set { } }
        public void Delete() { }
        public static Blip AddBlip(Vector3 target) { return new Blip(); }
    }

    public class WeaponInstance
    {
        public int Ammo { get; set; }
    }

    public class WeaponCollection
    {
        public WeaponInstance FromType(Weapon w) { return new WeaponInstance(); }
    }

    public class PedTasks
    {
        public bool AlwaysKeepTask { set { } }
        public void FightAgainst(Ped target) { }
        public void GoTo(Vector3 pos) { }
        public void FightAgainstHatedTargets(float radius) { }
        public void ClearAllImmediately() { }
    }

    public class Ped
    {
        public Vector3 Position { get; set; }
        public int Health { get; set; }
        public int MaxHealth { set { } }
        public int Armor { get; set; }
        public int Accuracy { set { } }
        public bool isAlive { get; set; }
        public bool isDead { get; set; }
        public bool isInCombat { get; set; }
        public bool Enemy { set { } }
        public bool WantedByPolice { set { } }
        public RelationshipGroup RelationshipGroup { get; set; }
        public PedTasks Task { get { return new PedTasks(); } }
        public WeaponCollection Weapons { get { return new WeaponCollection(); } }
        public void ChangeRelationship(RelationshipGroup g, Relationship r) { }
        public void MakeProofTo(bool bullets, bool fire, bool explosions, bool fallingDamage, bool melee) { }
        public void Die() { }
    }

    public class Player
    {
        public Ped Character { get { return new Ped(); } }
        public int Money { get; set; }
        public int WantedLevel { get; set; }
    }

    public class GameConsole
    {
        public void Print(string text) { }
    }

    public static class Game
    {
        public static Player LocalPlayer { get { return new Player(); } }
        public static GameConsole Console { get { return new GameConsole(); } }
        public static void DisplayText(string text) { }
        public static void DisplayText(string text, int duration) { }
    }

    public static class World
    {
        public static Vector3 GetNextPositionOnStreet(Vector3 pos) { return pos; }
        public static Ped CreatePed(Model model, Vector3 pos) { return new Ped(); }
        public static Ped CreatePed(Vector3 pos) { return new Ped(); }
        public static Ped[] GetAllPeds() { return new Ped[0]; }
        public static Ped GetClosestPed(Vector3 pos, float radius) { return new Ped(); }
        public static Ped[] GetPeds(Vector3 pos, float radius) { return new Ped[0]; }
    }

    public delegate void KeyEventHandler(object sender, KeyEventArgs e);

    public class KeyEventArgs : EventArgs
    {
        public Keys Key { get; set; }
    }

    public class Script
    {
        public int Interval { get; set; }
        public event EventHandler Tick;
        public event KeyEventHandler KeyDown;
        public void Wait(int ms) { }
    }
}
