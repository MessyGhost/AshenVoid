using System.Collections.Generic;

namespace AshenVoid.Content.NPCs.NightmareCorruption.Configs
{
    // Top-level container for all boss configurations
    public class BossConfig
    {
        public int LifeMax { get; set; }
        public float SpawnDuration { get; set; }
        public int Damage { get; set; }
        public int Defense { get; set; }
        public PhaseConfig Phase1 { get; set; }
        public List<AbilityConfig> Abilities { get; set; } = new();
        // public PhaseConfig Phase2 { get; set; } // Future-proofing
    }
}