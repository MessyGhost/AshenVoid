using AshenVoid.Core.ECS.AI;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace AshenVoid.Core.ECS.FSM
{
    /// <summary>
    /// Manages the states of an AI, handling transitions, updates, and network synchronization.
    /// </summary>
    public class StateMachine
    {
        public IState CurrentState { get; private set; }

        /// <summary>
        /// Transitions to a new state and syncs the change to clients.
        /// </summary>
        public void ChangeState(IState newState, Blackboard blackboard)
        {
            if (newState == null || newState == CurrentState)
                return;

            CurrentState?.Exit(blackboard);
            CurrentState = newState;
            CurrentState.Enter(blackboard);

            // Network Synchronization
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

        /// <summary>
        /// Updates the current state and handles transitions automatically.
        /// This should only be called on the server.
        /// </summary>
        public void Update(Blackboard blackboard)
        {
            if (CurrentState == null || Main.netMode == NetmodeID.MultiplayerClient)
                return;

            var nextState = CurrentState.Update(blackboard);

            if (nextState != CurrentState)
            {
                ChangeState(nextState, blackboard);
            }
        }
    }
}