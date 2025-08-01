using AshenVoid.Core.Events;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;

namespace AshenVoid.Core.ECS.Systems
{
    public class AnimationSystem : IComponentSystem
    {
        public IEnumerable<Type> RequiredComponents => new[] { typeof(AnimationComponent) };
        public SystemExecutionSide ExecutionSide => SystemExecutionSide.Client;

        public AnimationSystem()
        {
            EcsSystem.Instance.EventBus.Subscribe<AttackPerformedNetworkEvent>(HandleAttackAnimation);
        }

        public void Update(GameTime gameTime, int entityId, EcsWorld world, EventBus eventBus)
        {
            var animation = world.GetComponent<AnimationComponent>(entityId);
            if (animation == null) return;

            animation.FrameCounter++;
            if (animation.FrameCounter > animation.FrameDelay)
            {
                animation.FrameCounter = 0;
                animation.CurrentFrame++;
                if (animation.CurrentFrame >= Main.npcFrameCount[animation.Npc.type])
                {
                    animation.CurrentFrame = 0;
                }
            }
            animation.Npc.frame.Y = animation.CurrentFrame * animation.Npc.height;
        }

        private void HandleAttackAnimation(AttackPerformedNetworkEvent e)
        {
            var world = EcsSystem.Instance.World;
            var animation = world.GetComponent<AnimationComponent>(e.EntityId);

            // Example: Play a specific animation for the basic attack
            if (e.AttackId == 0)
            {
                // This is a placeholder. You would typically have a more robust
                // animation controller within the AnimationComponent.
                // For now, we can just manually set the frame.
                if (animation != null)
                {
                    animation.Npc.frameCounter = 0;
                    animation.Npc.frame.Y = animation.Npc.height * 1; // Assuming frame 1 is the attack frame
                }
            }
        }
    }
}