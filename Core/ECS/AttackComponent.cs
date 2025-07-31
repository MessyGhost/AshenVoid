using AshenVoid.Content.NPCs.NightmareCorruption.Configs;
using AshenVoid.Content.NPCs.NightmareCorruption.Intents;
using AshenVoid.Core.ECS.Interfaces;
using Terraria;
using Terraria.ID;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;

namespace AshenVoid.Core.ECS
{
    public interface IAttackIntent { }

    public class AttackComponent : IAttackComponent
    {
        private readonly NPC _npc;
        private IAttackIntent _currentIntent;
        private float _cooldownTimer; // Timer to track cooldown

        public AttackComponent(NPC npc) { _npc = npc; }

        public void SetIntent(IAttackIntent intent)
        {
            if (IsReady())
                _currentIntent = intent;
        }

        public bool IsReady() => _cooldownTimer <= 0;
        public bool IsAttacking() => _currentIntent != null; // True while an attack is being executed

        public void Update()
        {
            if (_cooldownTimer > 0)
                _cooldownTimer -= 1f / 60f; // Decrement by frame time

            if (_currentIntent == null) return;

            // Execute the attack based on intent type
            switch (_currentIntent)
            {
                case ShootProjectileIntent shoot:
                    ExecuteShootProjectile(shoot);
                    _cooldownTimer = shoot.Stats.Cooldown; // Set cooldown from stats
                    break;
            }

            _currentIntent = null; // Consume the intent
        }

        private void ExecuteShootProjectile(ShootProjectileIntent intent)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;

            Vector2 velocity = Vector2.Normalize(intent.TargetPosition - _npc.Center) * intent.Stats.Speed;
            Projectile.NewProjectile(
                _npc.GetSource_FromAI(),
                _npc.Center,
                velocity,
                intent.Stats.ProjectileId,
                (int)intent.Stats.Damage,
                0f,
                Main.myPlayer
            );
        }

        public void Reset()
        {
            _currentIntent = null;
            _cooldownTimer = 0;
        }
    }
}