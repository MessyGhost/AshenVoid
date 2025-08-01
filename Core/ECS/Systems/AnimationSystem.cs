using AshenVoid.Core.Events;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;

namespace AshenVoid.Core.ECS.Systems
{
    public class AnimationSystem : IAnimationSystem
    {
        private const int FrameDelay = 5; // Ticks between frame changes.

        public SystemExecutionSide ExecutionSide => SystemExecutionSide.Both;

        public HashSet<Type> RequiredComponents { get; } = new HashSet<Type>
        {
            typeof(AnimationComponent)
        };

        public void Update(GameTime gameTime, NPC npc, ComponentController controller, EventBus eventBus)
        {
            var animationComponent = controller.GetComponent<AnimationComponent>();

            animationComponent.FrameCounter++;
            if (animationComponent.FrameCounter >= FrameDelay)
            {
                animationComponent.FrameCounter = 0;
                animationComponent.CurrentFrame++;
                if (animationComponent.CurrentFrame >= Main.npcFrameCount[npc.type])
                {
                    animationComponent.CurrentFrame = 0;
                }
            }
            npc.frame.Y = animationComponent.CurrentFrame * npc.height;
        }
    }
}