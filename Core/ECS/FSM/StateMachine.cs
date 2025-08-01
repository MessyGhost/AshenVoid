using AshenVoid.Core.ECS.AI;
using AshenVoid.Core.ECS.BehaviorTree;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace AshenVoid.Core.ECS.FSM
{
    public class StateMachine
    {
        public IState CurrentState { get; private set; }
        private Node _activeBehaviorTree;

        private void PerformStateChange(IState newState, Blackboard blackboard)
        {
            if (newState == null || newState == CurrentState)
                return;

            CurrentState?.Exit(blackboard);
            _activeBehaviorTree = null;

            CurrentState = newState;
            CurrentState.Enter(blackboard);

            // Build behavior tree on the server, or if we are in single player.
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                _activeBehaviorTree = CurrentState.BuildBehaviorTree(blackboard);
            }
        }

        public void ChangeState(IState newState, Blackboard blackboard)
        {
            PerformStateChange(newState, blackboard);

            if (Main.netMode == NetmodeID.Server)
            {
                var npc = blackboard.Get<NPC>(BlackboardKeys.NPC);
                var aiState = blackboard.Get<AIStateComponent>(BlackboardKeys.AIState);
                int stateId = aiState.GetStateId(newState.GetType());

                if (stateId != -1)
                {
                    var packet = ModContent.GetInstance<AshenVoid>().GetPacket();
                    packet.Write((byte)AshenVoid.MessageType.SyncBossState);
                    packet.Write((byte)npc.whoAmI);
                    packet.Write((byte)stateId);
                    packet.Send();
                }
            }
        }

        public void ReceiveStateChange(IState newState, Blackboard blackboard)
        {
            // This method is called on the client to apply a state change from the server.
            // It does not send any packets.
            PerformStateChange(newState, blackboard);
        }

        public void Update(Blackboard blackboard)
        {
            if (CurrentState == null || Main.netMode == NetmodeID.MultiplayerClient)
                return;

            var nextState = CurrentState.CheckTransitions(blackboard);
            if (nextState != null)
            {
                ChangeState(nextState, blackboard);
            }
            else
            {
                _activeBehaviorTree?.Evaluate(blackboard);
            }
        }
    }
}