using AshenVoid.Core.ECS;
using Microsoft.Xna.Framework;
using Terraria;

namespace AshenVoid.Core.ECS.Systems
{
    public class AnimationSystem : ISystem
    {
        public void Update(GameTime gameTime, NPC npc)
        {
            var controller = (npc.ModNPC as Content.NPCs.NightmareCorruption.NightmareCorruption)?.ComponentController;
            if (controller == null) return;

            var animationComponent = controller.GetComponent<AnimationComponent>();
            if (animationComponent == null) return;

            // This logic will be driven by states or animation requests in the future.
            // For now, we can keep the simple movement-based animation.

            int frameHeight = npc.frame.Height;
            animationComponent.FrameCounter++;

            bool isMoving = npc.velocity.Length() > 1.5f;

            if (isMoving)
            {
                if (animationComponent.FrameCounter >= 10)
                {
                    npc.frame.Y = (npc.frame.Y + frameHeight) % (4 * frameHeight);
                    if (npc.frame.Y < 2 * frameHeight)
                    {
                        npc.frame.Y = 2 * frameHeight;
                    }
                    animationComponent.FrameCounter = 0;
                }
            }
            else
            {
                if (animationComponent.FrameCounter >= 15)
                {
                    npc.frame.Y = (npc.frame.Y + frameHeight) % (2 * frameHeight);
                    animationComponent.FrameCounter = 0;
                }
            }
        }
    }
}