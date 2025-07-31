using Terraria;

namespace AshenVoid.Core.ECS.FSM
{
    /// <summary>
    /// Manages the states of an AI, handling transitions and updates.
    /// </summary>
    public class StateMachine
    {
        public IState CurrentState { get; private set; }

        public StateMachine() { }

        /// <summary>
        /// Transitions to a new state.
        /// </summary>
        /// <param name="newState">The state to transition to.</param>
        public void ChangeState(IState newState, ComponentController controller, NPC npc)
        {
            CurrentState?.Exit();
            CurrentState = newState;
            CurrentState?.Enter(controller, npc);
        }

        /// <summary>
        /// Updates the current state.
        /// </summary>
        public void Update(ComponentController controller, NPC npc, Player target)
        {
            CurrentState?.Update(controller, npc, target);
        }
    }
}