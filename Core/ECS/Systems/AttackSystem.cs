using AshenVoid.Core.ECS.AI;
using AshenVoid.Core.ECS.Intents;
using AshenVoid.Core.Events;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;

namespace AshenVoid.Core.ECS.Systems
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(AIStateSystem))]
    public class AttackSystem : IComponentSystem
    {
        public SystemExecutionSide ExecutionSide => SystemExecutionSide.Server;

        public HashSet<Type> RequiredComponents { get; } = new HashSet<Type>
        {
            typeof(AttackComponent),
            typeof(StatSheetComponent),
            typeof(AIStateComponent)
        };

        public void Update(GameTime gameTime, NPC npc, ComponentController controller, EventBus eventBus)
        {
            var attackComponent = controller.GetComponent<AttackComponent>();
            var statSheet = controller.GetComponent<StatSheetComponent>();
            var aiState = controller.GetComponent<AIStateComponent>();
            var blackboard = aiState.Blackboard;

            if (attackComponent.CooldownTimer > 0)
            {
                attackComponent.CooldownTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            }

            if (!blackboard.TryGet(BlackboardKeys.AttackIntent, out IAttackIntent intent) || !attackComponent.IsReady())
            {
                return;
            }

            switch (intent)
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

            blackboard.Remove(BlackboardKeys.AttackIntent);
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
                (int)npc.damage,
                0f,
                Main.myPlayer
            );
        }
    }
}