using AshenVoid.Core.ECS.AI;
using System;

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
        public void ChangeState(IState newState, Blackboard blackboard)
        {
            CurrentState?.Exit(blackboard);
            CurrentState = newState;
            CurrentState?.Enter(blackboard);
        }

        /// <summary>
        /// Updates the current state.
        /// </summary>
        /// <returns>If a state transition is requested, the Type of the new state; otherwise, null.</returns>
        public Type Update(Blackboard blackboard)
        {
            return CurrentState?.Update(blackboard);
        }
    }
}