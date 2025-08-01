using AshenVoid.Content.NPCs.NightmareCorruption.Configs;
using AshenVoid.Core.ECS.AI;
using AshenVoid.Core.ECS.BehaviorTree;
using AshenVoid.Core.ECS.FSM;
using System;
using Terraria;

namespace AshenVoid.Content.NPCs.NightmareCorruption.States
{
    public class Phase1State : IState
    {
        private static readonly string BehaviorTreeKey = "ActiveBehaviorTree";
        private AIBehaviorFactory _behaviorFactory;

        public void Enter(Blackboard blackboard)
        {
            // Initialize the factory if it doesn't exist
            if (_behaviorFactory == null)
            {
                _behaviorFactory = new AIBehaviorFactory(blackboard);
            }

            // Create and store the behavior tree for this phase
            var behaviorTree = _behaviorFactory.CreateBehaviorTree("NightmareCorruption_Phase1");
            blackboard.Set(BehaviorTreeKey, behaviorTree);
        }

        public Type Update(Blackboard blackboard)
        {
            var npc = blackboard.Get<NPC>(BlackboardKeys.NPC);
            var config = blackboard.Get<BossConfig>("BossConfig").Phase1;

            // Check for phase transition first, as it's the highest priority.
            if (npc.life < npc.lifeMax * config.PhaseTransitionHealth)
            {
                return typeof(Phase2State);
            }

            // Evaluate the active behavior tree.
            var behaviorTree = blackboard.Get<Node>(BehaviorTreeKey);
            behaviorTree?.Evaluate();

            return null; // No transition requested by default
        }

        public void Exit(Blackboard blackboard)
        {
            // Clean up the active tree from the blackboard
            blackboard.Remove(BehaviorTreeKey);
        }
    }
}