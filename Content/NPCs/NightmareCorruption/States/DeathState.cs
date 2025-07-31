using AshenVoid.Core.ECS;
using AshenVoid.Core.ECS.FSM;
using Terraria;

namespace AshenVoid.Content.NPCs.NightmareCorruption.States
{
    public class DeathState : IState
    {
        public void Enter(ComponentController controller, NPC npc)
        {
            // Logic for the death sequence will be implemented here.
        }

        public void Update(ComponentController controller, NPC npc, Player target)
        {
            // This state will handle the multi-stage death animation.
        }

        public void Exit() { }
    }
}