using AshenVoid.Core.Stats; // Reference the content-agnostic stats from Core

namespace AshenVoid.Content.NPCs.NightmareCorruption.Configs
{
    // Contains all parameters for a specific phase
    public class PhaseConfig
    {
        public float PhaseTransitionHealth { get; set; }
        public MovementStats Movement { get; set; } // This will now correctly resolve to Core.Stats.MovementStats
        public AttackStats Attacks { get; set; }
        public DashStats Dash { get; set; }
        public SummonStats Summon { get; set; }
    }
}