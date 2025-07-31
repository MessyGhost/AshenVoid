using AshenVoid.Core.ECS;

namespace AshenVoid.Core.ECS.Interfaces
{
    public interface IVFXComponent : IComponent
    {
        void AddEffect(IVFXEffect effect);
        void RemoveEffect<T>() where T : IVFXEffect;
    }
}