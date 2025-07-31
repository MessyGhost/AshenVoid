using AshenVoid.Core.ECS;

namespace AshenVoid.Core.ECS.Interfaces
{
    public interface IAnimationComponent : IComponent
    {
        void SetAnimation(string animationName);
        void SetFrameRate(float frameRate);
    }
}