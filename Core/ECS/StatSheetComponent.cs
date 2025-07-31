using AshenVoid.Content.NPCs.NightmareCorruption.Configs;
using AshenVoid.Core.ECS.Interfaces;
using AshenVoid.Core.Stats;
using Terraria;

namespace AshenVoid.Core.ECS
{
    public class StatSheetComponent : IStatSheetComponent
    {
        public ModifiableStat Damage { get; }
        public ModifiableStat Defense { get; }
        public ModifiableStat MovementSpeed { get; }
        public ModifiableStat AttackCooldownMultiplier { get; }

        private readonly NPC _npc;

        public StatSheetComponent(NPC npc, BossConfig config)
        {
            _npc = npc;

            // Initialize stats from the config file
            Damage = new ModifiableStat(config.Damage);
            Defense = new ModifiableStat(config.Defense);
            MovementSpeed = new ModifiableStat(config.Phase1.Movement.MaxSpeed);
            AttackCooldownMultiplier = new ModifiableStat(1f); // Default to 1 (no modification)
        }
    }
}