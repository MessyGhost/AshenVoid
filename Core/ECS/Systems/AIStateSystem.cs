using AshenVoid.Core.ECS;
using Microsoft.Xna.Framework;
using Terraria;

namespace AshenVoid.Core.ECS.Systems
{
    public class AIStateSystem : ISystem
    {
        public void Update(GameTime gameTime, NPC npc)
        {
            var controller = (npc.ModNPC as Content.NPCs.NightmareCorruption.NightmareCorruption)?.ComponentController;
            if (controller == null) return;

            var aiState = controller.GetComponent<AIStateComponent>();
            if (aiState == null) return;

            // Update target
            if (npc.target < 0 || npc.target == 255 || Main.player[npc.target].dead || !Main.player[npc.target].active)
            {
                npc.TargetClosest(true);
            }
            aiState.Target = Main.player[npc.target];

            // Update the state machine
            aiState.StateMachine.Update(controller, npc, aiState.Target);
        }
    }
}