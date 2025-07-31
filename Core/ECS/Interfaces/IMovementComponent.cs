using AshenVoid.Core.ECS;

namespace AshenVoid.Core.ECS.Interfaces
{
    public interface IMovementComponent : IComponent
    {
        void SetIntent(IMovementIntent intent);
        bool IsMoving();
    }
}