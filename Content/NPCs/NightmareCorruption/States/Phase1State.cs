using AshenVoid.Content.NPCs.NightmareCorruption.Configs;
using AshenVoid.Core.ECS.AI;
using AshenVoid.Core.ECS.BehaviorTree;
using AshenVoid.Core.ECS.FSM;
using Terraria;

namespace AshenVoid.Content.NPCs.NightmareCorruption.States
{
    public class Phase1State : IState
    {
        public void Enter(Blackboard blackboard)
        {
            // Initialization logic for Phase 1, if any, goes here.
            // For example, setting specific movement parameters.
        }

        public void Exit(Blackboard blackboard)
        {
            // Cleanup logic for Phase 1.
        }

        public Node BuildBehaviorTree(Blackboard blackboard)
        {
            var behaviorFactory = blackboard.Get<AIBehaviorFactory>(BlackboardKeys.AIBehaviorFactory);
            // The actual logic of phase 1 is now fully encapsulated within this behavior tree.
            return behaviorFactory.CreateBehaviorTree("NightmareCorruption_Phase1");
        }

        public IState CheckTransitions(Blackboard blackboard)
        {
            var npc = blackboard.Get<NPC>(BlackboardKeys.NPC);
            var config = blackboard.Get<BossConfig>("BossConfig").Phase1;

            // Check for health-based transition to Phase 2
            if (npc.life < npc.lifeMax * config.PhaseTransitionHealth)
            {
                var stateFactory = blackboard.Get<StateFactory>(BlackboardKeys.StateFactory);
                return stateFactory.GetState<Phase2State>();
            }

            // No transition needed
            return null;
        }
    }
}