using AshenVoid.Core.ECS.Interfaces;
using Terraria;

namespace AshenVoid.Core.ECS
{
    public class AnimationComponent : IAnimationComponent
    {
        public int CurrentFrame { get; set; }
        public int FrameCounter { get; set; }

        public AnimationComponent(NPC npc)
        {
            // The NPC reference is no longer needed here,
            // as the system will provide it.
        }

        public void SetAnimation(string animationName)
        {
            // TODO: Implement state-driven animation logic
        }

        public void SetFrameRate(float frameRate)
        {
            // TODO: Implement frame rate control
        }

        public void Update()
        {
            // All logic is moved to AnimationSystem.
        }
    }
}