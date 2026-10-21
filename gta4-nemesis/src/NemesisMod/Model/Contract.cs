namespace GTA4Nemesis.Model
{
    public enum ContractType
    {
        Bounty,          // hunt a specific named criminal / gang member
        Assassination    // kill a named target for a client
    }

    /// <summary>
    /// A generated side activity: bounty or assassination contract.
    /// </summary>
    public class Contract
    {
        public string Id;
        public ContractType Type;
        public string TargetMemberId;  // member being hunted
        public string TargetLabel;     // display name of target
        public int Reward;
        public bool Active;
        public float X, Y, Z;          // current reported position

        public Contract() { }
    }
}
