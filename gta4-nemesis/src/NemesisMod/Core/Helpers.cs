using System;
using GTA;

namespace GTA4Nemesis.Core
{
    /// <summary>
    /// Central wrapper around the ScriptHookDotNet GTA API. All game-facing
    /// calls live here so a version-specific API difference is fixed once.
    /// Verified against ScriptHookDotNet v1.7.1.8 (Complete Edition fork).
    /// </summary>
    public static class Helpers
    {
        public static readonly Random Rng = new Random();

        public static void Safe(Action a)
        {
            try { a(); } catch { }
        }

        public static Ped Player
        {
            get { Ped p = null; Safe(() => p = Game.LocalPlayer.Character); return p; }
        }

        public static Vector3 PlayerPos
        {
            get { Vector3 v = new Vector3(0f, 0f, 0f); Safe(() => v = Game.LocalPlayer.Character.Position); return v; }
        }

        /// <summary>Snap a rough position to a valid street point.</summary>
        public static Vector3 SnapToStreet(Vector3 pos)
        {
            Vector3 r = pos;
            Safe(() => r = World.GetNextPositionOnStreet(pos));
            return r;
        }

        public static Ped SpawnPed(string modelName, Vector3 pos)
        {
            Ped p = null;
            if (!string.IsNullOrEmpty(modelName))
                Safe(() => { GTA.Model m = modelName; p = World.CreatePed(m, SnapToStreet(pos)); });
            if (p == null)
                Safe(() => p = World.CreatePed(SnapToStreet(pos)));
            return p;
        }

        public static Blip CreateBlip(Vector3 pos, BlipColor color, bool friendly)
        {
            Blip b = null;
            Safe(() =>
            {
                b = Blip.AddBlip(pos);
                if (b != null) { b.Color = color; b.Friendly = friendly; }
            });
            return b;
        }

        public static void ShowMessage(string text, int ms = 3000)
        {
            Safe(() => Game.DisplayText(text, ms));
            Safe(() => Game.Console.Print(text));
        }

        public static void GiveWeapon(Ped p, Weapon w, int ammo)
        {
            Safe(() => p.Weapons.FromType(w).Ammo = ammo);
        }

        public static void ArmPed(Ped p, int tier)
        {
            if (tier <= 0)
            {
                GiveWeapon(p, Weapon.Handgun_Glock, 120);
            }
            else if (tier == 1)
            {
                GiveWeapon(p, Weapon.SMG_MP5, 240);
                GiveWeapon(p, Weapon.Handgun_Glock, 120);
            }
            else
            {
                GiveWeapon(p, Weapon.Rifle_AK47, 300);
                GiveWeapon(p, Weapon.SMG_MP5, 240);
            }
        }

        /// <summary>Health above 100 requires raising MaxHealth first.</summary>
        public static void SetHealth(Ped p, int value)
        {
            Safe(() => { p.MaxHealth = value; p.Health = value; });
        }

        public static void SetHostileToPlayer(Ped p)
        {
            Safe(() =>
            {
                p.ChangeRelationship(RelationshipGroup.Player, Relationship.Hate);
                p.Enemy = true;
            });
        }

        public static void SetFriendlyToPlayer(Ped p)
        {
            Safe(() =>
            {
                p.RelationshipGroup = RelationshipGroup.Player;
                p.ChangeRelationship(RelationshipGroup.Player, Relationship.Companion);
            });
        }

        public static float Dist(Vector3 a, Vector3 b)
        {
            float dx = a.X - b.X, dy = a.Y - b.Y, dz = a.Z - b.Z;
            return (float)Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        public static bool Near(Vector3 a, Vector3 b, float meters)
        {
            return Dist(a, b) <= meters;
        }

        /// <summary>Random street point in a ring around the player.</summary>
        public static Vector3 RandomStreetNearPlayer(float minDist, float maxDist)
        {
            Vector3 p = PlayerPos;
            double ang = Rng.NextDouble() * Math.PI * 2.0;
            float dist = minDist + (float)(Rng.NextDouble() * (maxDist - minDist));
            Vector3 pos = new Vector3(
                p.X + (float)(Math.Cos(ang) * dist),
                p.Y + (float)(Math.Sin(ang) * dist),
                p.Z);
            return SnapToStreet(pos);
        }
    }
}
