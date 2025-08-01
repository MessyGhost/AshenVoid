namespace AshenVoid.Core.ECS.AI
{
    public static class BlackboardKeys
    {
        // Core
        public const string NPC = "NPC";
        public const string Target = "Target";
        public const string GameTime = "GameTime";
        public const string AIState = "AIState";
        public const string ServiceLocator = "ServiceLocator";

        // Factories & Services
        public const string StateFactory = "StateFactory";
        public const string AIBehaviorFactory = "AIBehaviorFactory";

        // Intents
        public const string MovementIntent = "MovementIntent";
        public const string AttackIntent = "AttackIntent";

        // Timers
        public const string StateTimer = "StateTimer";
        public const string AttackTimer = "AttackTimer";
    }
}