using AshenVoid.Core.ECS.AI;
using Microsoft.Xna.Framework;
using Terraria;

namespace AshenVoid.Core.ECS.Systems
{
    public class AIStateSystem : IAIStateSystem
    {
        public void Update(GameTime gameTime, NPC npc, AIStateComponent aiState)
        {
            // Update target
            if (npc.target < 0 || npc.target == 255 || Main.player[npc.target].dead || !Main.player[npc.target].active)
            {
                npc.TargetClosest(true);
            }
            Player target = Main.player[npc.target];

            // Populate the blackboard with this frame's context
            var blackboard = aiState.Blackboard;
            blackboard.Set(BlackboardKeys.Target, target);
            blackboard.Set(BlackboardKeys.GameTime, gameTime);

            // This is the key change: Make the blackboard accessible to other systems
            // and prepare for removing direct controller access from states.
            blackboard.Set(BlackboardKeys.Blackboard, blackboard);

            // The rest of the keys (NPC, Controller, AIState) should already be set
            // during initialization in AIStateComponent. We will remove Controller soon.

            // Update the state machine with the blackboard
            aiState.StateMachine.Update(blackboard);
        }
    }
}