using AshenVoid.Core.ECS;

namespace AshenVoid.Core.ECS.Interfaces
{
    public interface IAttackComponent : IComponent
    {
        void SetIntent(IAttackIntent intent);
        bool IsReady();
        bool IsAttacking();
    }
}