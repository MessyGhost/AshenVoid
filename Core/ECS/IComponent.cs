namespace AshenVoid.Core.ECS
{
    /// <summary>
    /// A marker interface for all components in the ECS-like architecture.
    /// In the new architecture, components have specific roles and are not all required to have the same methods.
    /// We will add a generic Update method for components that need per-frame logic.
    /// </summary>
    public interface IComponent
    {
        /// <summary>
        /// Called every frame by the ComponentController.
        /// </summary>
        void Update();
    }
}