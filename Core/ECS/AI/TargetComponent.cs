using Terraria;

namespace AshenVoid.Core.ECS.AI
{
    public class TargetComponent : IComponent
    {
        public Player Target { get; set; }

        public TargetComponent(Player target = null)
        {
            Target = target;
        }
    }
}