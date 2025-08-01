using AshenVoid.Core.Stats;

namespace AshenVoid.Content.NPCs.NightmareCorruption.Configs
{
    public class PhaseConfig
    {
        public float PhaseTransitionHealth { get; set; }
        public float ChaseDistanceThreshold { get; set; }
        public MovementStats Movement { get; set; }
        public AttackStats Attacks { get; set; }
        public DashStats Dash { get; set; }
        public SummonStats Summon { get; set; }
        public BehaviorTreeConfig BehaviorTree { get; set; }
    }
}