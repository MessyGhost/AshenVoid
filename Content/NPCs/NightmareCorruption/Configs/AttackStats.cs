namespace AshenVoid.Content.NPCs.NightmareCorruption.Configs
{
    // Contains parameters for all attacks in a phase
    public class AttackStats
    {
        public ProjectileAttack BasicShot { get; set; }
        // public BeamAttack DeathRay { get; set; } // Example for other attack types
    }

    // Example for a specific attack type
    public class ProjectileAttack
    {
        public int ProjectileId { get; set; }
        public float Cooldown { get; set; }
        public float Damage { get; set; }
        public float Speed { get; set; }
    }
}