namespace AshenVoid.Content.NPCs.NightmareCorruption.Configs
{
    // Contains all parameters for a specific phase
    public class PhaseConfig
    {
        public MovementStats Movement { get; set; }
        public AttackStats Attacks { get; set; }
        public DashStats Dash { get; set; }
        public SummonStats Summon { get; set; }
    }
}