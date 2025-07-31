using AshenVoid.Core.ECS.Interfaces;
using Terraria;

namespace AshenVoid.Core.ECS
{
    public class AnimationComponent : IAnimationComponent
    {
        private readonly NPC _npc;
        private int _frameCounter;

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
            // This logic will be driven by states or animation requests in the future.
            // For now, we can keep the simple movement-based animation.

            int frameHeight = _npc.frame.Height;
            _frameCounter++;

            bool isMoving = _npc.velocity.Length() > 1.5f;

            if (isMoving)
            {
                if (_frameCounter >= 10)
                {
                    _npc.frame.Y = (_npc.frame.Y + frameHeight) % (4 * frameHeight);
                    if (_npc.frame.Y < 2 * frameHeight)
                    {
                        _npc.frame.Y = 2 * frameHeight;
                    }
                    _frameCounter = 0;
                }
            }
            else
            {
                if (_frameCounter >= 15)
                {
                    _npc.frame.Y = (_npc.frame.Y + frameHeight) % (2 * frameHeight);
                    _frameCounter = 0;
                }
            }
        }
    }
}