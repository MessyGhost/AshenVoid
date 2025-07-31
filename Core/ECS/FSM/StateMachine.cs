using AshenVoid.Core.ECS.AI;

namespace AshenVoid.Core.ECS.FSM
{
    /// <summary>
    /// Manages the states of an AI, handling transitions and updates.
    /// </summary>
    public class StateMachine
    {
        public IState CurrentState { get; private set; }

        /// <summary>
        /// Transitions to a new state.
        /// </summary>
        /// <param name="newState">The state to transition to.</param>
        /// <param name="blackboard">The AI's blackboard containing all context.</param>
        public void ChangeState(IState newState, Blackboard blackboard)
        {
            CurrentState?.Exit(blackboard);
            CurrentState = newState;
            CurrentState?.Enter(blackboard);
        }

        /// <summary>
        /// Updates the current state.
        /// </summary>
        /// <param name="blackboard">The AI's blackboard containing all context.</param>
        public void Update(Blackboard blackboard)
        {
            CurrentState?.Update(blackboard);
        }
    }
}