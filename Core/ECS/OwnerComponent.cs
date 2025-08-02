namespace AshenVoid.Core.ECS
{
    /// <summary>
    /// A component that marks an entity as being owned by another entity.
    /// Used for minions, projectiles, etc.
    /// Using a struct to avoid heap allocation.
    /// </summary>
    public struct OwnerComponent : IComponent
    {
        public int OwnerEntityId;
    }
}