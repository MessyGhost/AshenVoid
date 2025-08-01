namespace AshenVoid.Core.Stats
{
    /// <summary>
    /// A reusable data structure for holding movement-related parameters.
    /// This is content-agnostic and can be used by any NPC.
    /// </summary>
    public class MovementStats
    {
        // Second Order Dynamics parameters
        public float Frequency { get; set; } = 1.2f;
        public float DampingRatio { get; set; } = 0.8f;
        public float ResponseScale { get; set; } = 0.5f;

        // AI behavior parameters
        public float PatrolSpeed { get; set; } = 8f;
        public float ChaseDistanceFar { get; set; } = 800f;
        public float ChaseStopDistance { get; set; } = 400f;
        public float OrbitRadius { get; set; } = 300f;
        public float ThinkIntervalMin { get; set; } = 2f;
        public float ThinkIntervalMax { get; set; } = 4f;
        public float MaxSpeed { get; set; } = 15f;
    }
}