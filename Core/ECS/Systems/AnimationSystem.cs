using AshenVoid.Core.Events;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;

namespace AshenVoid.Core.ECS.Systems
{
    public class AnimationSystem : CachedComponentSystem
    {
        public override IEnumerable<Type> RequiredComponents => new[] { typeof(AnimationComponent) };
        public override SystemExecutionSide ExecutionSide => SystemExecutionSide.Client;

        public AnimationSystem()
        {
            EcsSystem.Instance.EventBus.Subscribe<RequestAttackExecutionEvent>(HandleAttackAnimation);
        }

        public override void UpdateEntity(GameTime gameTime, int entityId, EcsWorld world, EventBus eventBus)
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

        private void HandleAttackAnimation(RequestAttackExecutionEvent e)
        {
            var world = EcsSystem.Instance.World;
            var animation = world.GetComponent<AnimationComponent>(e.EntityId);

            if (e.AttackType == AttackType.BasicShot)
            {
                if (animation != null)
                {
                    animation.Npc.frameCounter = 0;
                    animation.Npc.frame.Y = animation.Npc.height * 1; // Assuming frame 1 is the attack frame
                }
            }
        }
    }
}