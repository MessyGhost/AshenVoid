namespace AshenVoid.Core.ECS.Systems
{
    /// <summary>
    /// A marker interface for all systems in the ECS architecture.
    /// Systems contain logic that operates on components.
    /// A system should have one public method named "Update" which parameters will be injected.
    /// </summary>
    public interface ISystem
    {
    }
}