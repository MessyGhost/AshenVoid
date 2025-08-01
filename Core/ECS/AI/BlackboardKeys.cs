namespace AshenVoid.Core.ECS.AI
{
    /// <summary>
    /// Defines constant keys for accessing data in the Blackboard to avoid magic strings.
    /// </summary>
    public static class BlackboardKeys
    {
        public const string Blackboard = "Blackboard";
        public const string MovementIntent = "MovementIntent";
        public const string AttackIntent = "AttackIntent";
        public const string NPC = "NPC";
        public const string Target = "Target";
        public const string Controller = "Controller";
        public const string GameTime = "GameTime";
        public const string AIState = "AIState";
        public const string StateFactory = "StateFactory"; // For accessing the state factory
        public const string AIBehaviorFactory = "AIBehaviorFactory"; // For accessing the behavior factory
        public const string NextStateIntent = "NextStateIntent"; // For behavior tree nodes to request a state change
    }
}