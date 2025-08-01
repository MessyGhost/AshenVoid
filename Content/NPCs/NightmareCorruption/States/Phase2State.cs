using AshenVoid.Core;
using AshenVoid.Core.ECS.AI;
using AshenVoid.Core.ECS.BehaviorTree;
using AshenVoid.Core.ECS.FSM;
using Terraria;

namespace AshenVoid.Content.NPCs.NightmareCorruption.States
{
    public class Phase2State : IState
    {
        public void Enter(Blackboard blackboard)
        {
            // Logic for entering Phase 2
        }

        public void Exit(Blackboard blackboard)
        {
            // Logic for exiting Phase 2
        }

        public Node BuildBehaviorTree(Blackboard blackboard)
        {
            return new SequenceNode(
                new ActionNode(bb => {
                    // TODO: Implement Phase 2 logic
                    return NodeState.Running;
                })
            );
        }

        public IState CheckTransitions(Blackboard blackboard)
        {
            var npc = blackboard.Get<NPC>(BlackboardKeys.NPC);

            if (npc.life <= 1)
            {
                var stateFactory = blackboard.Get<ServiceLocator>(BlackboardKeys.ServiceLocator).Get<StateFactory>();
                return stateFactory.GetState<DeathState>();
            }

            return null;
        }
    }
}