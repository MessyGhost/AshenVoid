using AshenVoid.Core.ECS.Intents;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace AshenVoid.Core.ECS.Systems
{
    public class AttackSystem : ISystem
    {
        public void Update(GameTime gameTime, NPC npc, AttackComponent attackComponent, StatSheetComponent statSheet)
        {
            // Update cooldown timer
            if (attackComponent.CooldownTimer > 0)
            {
                attackComponent.CooldownTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            }

            // Check if there's an attack to execute
            if (attackComponent.CurrentIntent == null || !attackComponent.IsReady()) return;

            // Execute the attack based on intent type
            switch (attackComponent.CurrentIntent)
            {
                case ShootProjectileIntent shoot:
                    ExecuteShootProjectile(npc, shoot);
                    attackComponent.CooldownTimer = shoot.Stats.Cooldown * statSheet.AttackCooldownMultiplier.Value;
                    break;

                case SpawnNpcIntent spawn:
                    ExecuteSpawnNpc(npc, spawn);
                    attackComponent.CooldownTimer = spawn.Cooldown * statSheet.AttackCooldownMultiplier.Value;
                    break;
            }

            // Consume the intent
            attackComponent.CurrentIntent = null;
        }

        private void ExecuteSpawnNpc(NPC npc, SpawnNpcIntent intent)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;

            for (int i = 0; i < intent.Count; i++)
            {
                NPC.NewNPC(npc.GetSource_FromAI(), (int)intent.SpawnPosition.X, (int)intent.SpawnPosition.Y, intent.NpcId);
            }
        }

        private void ExecuteShootProjectile(NPC npc, ShootProjectileIntent intent)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;

            Vector2 velocity = Vector2.Normalize(intent.TargetPosition - npc.Center) * intent.Stats.Speed;
            Projectile.NewProjectile(
                npc.GetSource_FromAI(),
                npc.Center,
                velocity,
                intent.Stats.ProjectileId,
                (int)npc.damage, // Use the NPC's final damage
                0f,
                Main.myPlayer
            );
        }
    }
}