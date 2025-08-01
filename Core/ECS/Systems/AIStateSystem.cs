using AshenVoid.Core.ECS.AI;
using AshenVoid.Core.Events;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;

namespace AshenVoid.Core.ECS.Systems
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public class AIStateSystem : IComponentSystem
    {
        public SystemExecutionSide ExecutionSide => SystemExecutionSide.Server;

        public HashSet<Type> RequiredComponents { get; } = new HashSet<Type>
        {
            typeof(AIStateComponent)
        };

        public void Update(GameTime gameTime, NPC npc, ComponentController controller, EventBus eventBus)
        {
            var aiState = controller.GetComponent<AIStateComponent>();

            // Ensure there's a valid target
            if (npc.target < 0 || npc.target == 255 || Main.player[npc.target].dead || !Main.player[npc.target].active)
            {
                npc.TargetClosest(true);
                if (npc.target < 0 || npc.target == 255)
                {
                    return;
                }
            }
            
            Player target = Main.player[npc.target];

            var blackboard = aiState.Blackboard;
            blackboard.Set(BlackboardKeys.Target, target);
            blackboard.Set(BlackboardKeys.GameTime, gameTime);

            aiState.StateMachine.Update(blackboard);
        }
    }
}