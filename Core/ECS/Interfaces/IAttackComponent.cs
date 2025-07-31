using AshenVoid.Core.ECS;
using AshenVoid.Core.ECS.Intents;

namespace AshenVoid.Core.ECS.Interfaces
{
    public interface IAttackComponent : IComponent
    {
        void SetIntent(IAttackIntent intent);
        bool IsReady();
        bool IsAttacking();
    }
}