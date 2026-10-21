namespace GTA4Nemesis.Model
{
    /// <summary>
    /// A contested region of Liberty City used by the gang-war system.
    /// </summary>
    public class Territory
    {
        public string Id;
        public string Name;
        public string ControllingGangId; // null = unclaimed
        public float X, Y, Z;            // center point
        public float Radius;             // capture radius in meters

        public Territory() { }

        public Territory(string id, string name, float x, float y, float z, float radius)
        {
            Id = id;
            Name = name;
            X = x; Y = y; Z = z;
            Radius = radius;
        }
    }
}
