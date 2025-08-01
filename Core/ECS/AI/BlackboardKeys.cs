namespace AshenVoid.Core.ECS.AI
{
    public static class BlackboardKeys
    {
        // Most keys are now obsolete as dependencies are injected
        // and data is passed directly or stored in components.
        public const string Target = "Target";
        public const string Owner = "Owner";
    }
}