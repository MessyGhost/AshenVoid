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
    }
}