using AshenVoid.Core.ECS.AI;
using Microsoft.Xna.Framework;
using Terraria;

namespace AshenVoid.Core.ECS.Systems
{
    public class AIStateSystem : ISystem
    {
        public void Update(GameTime gameTime, NPC npc)
        {
            var controller = (npc.ModNPC as IComponentProvider)?.ComponentController;
            if (controller == null) return;

            var aiState = controller.GetComponent<AIStateComponent>();
            if (aiState == null) return;

            // Update target
            if (npc.target < 0 || npc.target == 255 || Main.player[npc.target].dead || !Main.player[npc.target].active)
            {
                npc.TargetClosest(true);
            }
            Player target = Main.player[npc.target];

            // Populate the blackboard with this frame's context
            var blackboard = aiState.Blackboard;
            blackboard.Set(BlackboardKeys.NPC, npc);
            blackboard.Set(BlackboardKeys.Target, target);
            blackboard.Set(BlackboardKeys.Controller, controller);
            blackboard.Set(BlackboardKeys.GameTime, gameTime);
            blackboard.Set(BlackboardKeys.AIState, aiState);

            // Update the state machine with the blackboard
            aiState.StateMachine.Update(blackboard);
        }
    }
}