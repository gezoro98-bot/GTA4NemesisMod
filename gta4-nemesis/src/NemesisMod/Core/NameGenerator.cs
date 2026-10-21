using System;
using System.Collections.Generic;

namespace GTA4Nemesis.Core
{
    /// <summary>
    /// Generates persistent names for gang members so the Nemesis system
    /// can refer to enemies by name across sessions.
    /// </summary>
    public static class NameGenerator
    {
        static readonly string[] First =
        {
            "Viktor", "Dmitri", "Sergei", "Andrei", "Boris", "Ivan", "Nikolai",
            "Mikhail", "Yuri", "Alexei", "Chen", "Lao", "Wei", "Huang", "Marco",
            "Angelo", "Sal", "Vito", "Dante", "Tony", "Frank", "Joey", "Ricky",
            "Marcus", "Tyrone", "Jamal", "Dre", "Rasheed", "Kofi", "Malik",
            "Liam", "Declan", "Sean", "Paddy", "Mick", "Niall", "Shane"
        };

        static readonly string[] Last =
        {
            "Volkov", "Petrov", "Kozlov", "Sokolov", "Orlov", "Ivanov", "Morozov",
            "Zaitsev", "Kuznetsov", "Fedorov", "Wong", "Zhang", "Chen", "Liu",
            "Moretti", "Romano", "Russo", "Capello", "Vernetti", "Bellini",
            "Johnson", "Brown", "Carter", "Doyle", "Mason", "Reed", "Hughes",
            "Murphy", "Kelly", "O'Brien", "Byrne", "Gallagher", "Banks", "Ford"
        };

        static readonly string[] Epithet =
        {
            "the Butcher", "the Ghost", "Two-Finger", "the Snake", "the Bear",
            "Lucky", "the Widowmaker", "Mad Dog", "the Rat", "Iron Hands",
            "the Crow", "No-Eyes", "the Shark", "Scarface", "the Reaper"
        };

        public static string Generate(Random rng, string existing)
        {
            if (!string.IsNullOrEmpty(existing)) return existing;
            string name = First[rng.Next(First.Length)] + " " + Last[rng.Next(Last.Length)];
            if (rng.Next(3) == 0) name += " \"" + Epithet[rng.Next(Epithet.Length)] + "\"";
            return name;
        }
    }
}
