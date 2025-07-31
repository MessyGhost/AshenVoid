using AshenVoid.Core.ECS.Interfaces;
using Terraria;

namespace AshenVoid.Core.ECS
{
    public class AnimationComponent : IAnimationComponent
    {
        private readonly NPC _npc;
        public int FrameCounter { get; set; }

        public AnimationComponent(NPC npc)
        {
            _npc = npc;
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