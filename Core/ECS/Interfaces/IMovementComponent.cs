using AshenVoid.Core.ECS;
using AshenVoid.Core.ECS.Intents;

namespace AshenVoid.Core.ECS.Interfaces
{
    public interface IMovementComponent : IComponent
    {
        void SetIntent(IMovementIntent intent);
        bool IsMoving();
    }
}