namespace GTA4Nemesis.Model
{
    /// <summary>
    /// A persistent gang (or friendly faction) that owns members and territories.
    /// </summary>
    public class Gang
    {
        public string Id;
        public string Name;
        public string RelationshipGroupName; // name used for GTA relationship group
        public bool PlayerAlly;              // fights on the player's side
        public int ColorR, ColorG, ColorB;   // blip / HUD color

        public Gang() { }

        public Gang(string id, string name, string relGroup, bool ally, int r, int g, int b)
        {
            Id = id;
            Name = name;
            RelationshipGroupName = relGroup;
            PlayerAlly = ally;
            ColorR = r;
            ColorG = g;
            ColorB = b;
        }
    }
}
