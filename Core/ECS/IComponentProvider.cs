namespace AshenVoid.Core.ECS
{
    public interface IComponentProvider
    {
        T GetComponent<T>() where T : class, IComponent;
        bool TryGetComponent<T>(out T result) where T : class, IComponent;
    }
}