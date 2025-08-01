using AshenVoid.Core.ECS.AI;

namespace AshenVoid.Core.ECS.FSM
{
    /// <summary>
    /// Defines a stateless finite state machine (FSM) state.
    /// The state class itself should not contain any data that changes over time. All instance data should be stored in the Blackboard.
    /// This allows a single state instance to be safely shared by multiple NPCs.
    /// </summary>
    public interface IState
    {
        /// <summary>
        /// Called once when the state machine enters this state.
        /// Used to initialize data required for this state in the Blackboard (e.g., timers, counters).
        /// </summary>
        void Enter(Blackboard blackboard);

        /// <summary>
        /// Called every frame while this state is active.
        /// This is where the state logic is executed.
        /// <returns>The next state to transition to. Return 'this' if no transition is needed.</returns>
        /// </summary>
        IState Update(Blackboard blackboard);

        /// <summary>
        /// Called once when the state machine leaves this state.
        /// Used to clean up data set by this state from the Blackboard (if necessary).
        /// </summary>
        void Exit(Blackboard blackboard);
    }
}