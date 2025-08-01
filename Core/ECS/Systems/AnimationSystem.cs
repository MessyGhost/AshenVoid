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

        public void Update(GameTime gameTime, int entityId, EcsWorld world, EventBus eventBus)
        {
            var animation = world.GetComponent<AnimationComponent>(entityId);
            if (animation == null) return;

            // The logic from AnimationComponent.Update is now here.
            animation.FrameCounter++;
            if (animation.FrameCounter >= animation.FrameDelay)
            {
                animation.FrameCounter = 0;
                animation.CurrentFrame = (animation.CurrentFrame + 1) % Main.npcFrameCount[animation.Npc.type];
            }
            animation.Npc.frame.Y = animation.CurrentFrame * animation.Npc.height;
        }
    }
}