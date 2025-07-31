namespace AshenVoid.Content.NPCs.NightmareCorruption.Configs
{
    // Top-level container for all boss configurations
    public class BossConfig
    {
        public float SpawnDuration { get; set; }
        public int Damage { get; set; }
        public int Defense { get; set; }
        public PhaseConfig Phase1 { get; set; }
        // public PhaseConfig Phase2 { get; set; } // Future-proofing
    }
}