namespace AshenVoid.Core.ECS.Interfaces
{
    /// <summary>
    /// Represents a component that requires an initialization step after being constructed.
    /// This is useful for setting up event subscriptions or other dependencies that
    /// cannot be handled in the constructor.
    /// </summary>
    public interface IInitializable
    {
        void Initialize();
    }
}