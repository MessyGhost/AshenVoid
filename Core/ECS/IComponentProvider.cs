namespace AshenVoid.Core.ECS
{
    /// <summary>
    /// Defines an entity that can provide a ComponentController.
    /// This interface acts as a bridge between the Core systems and Content implementations (like ModNPCs),
    /// allowing systems to access components without needing to know the concrete type of the NPC.
    /// </summary>
    public interface IComponentProvider
    {
        ComponentController ComponentController { get; }
    }
}