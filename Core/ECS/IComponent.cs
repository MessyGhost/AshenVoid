namespace AshenVoid.Core.ECS
{
    /// <summary>
    /// A marker interface for all components in the ECS architecture.
    /// Components are pure data containers and should not contain logic.
    /// </summary>
    public interface IComponent
    {
        // This is now a marker interface. All update logic belongs in Systems.
    }
}