using AshenVoid.Core.ECS.AI;

namespace AshenVoid.Core.ECS.FSM
{
    /// <summary>
    /// Defines a state for a Finite State Machine (FSM).
    /// Each state represents a major phase or behavior of an NPC.
    /// States are stateless and operate on data provided by a Blackboard.
    /// </summary>
    public interface IState
    {
        /// <summary>
        /// Called once when the state machine transitions into this state.
        /// Use for initialization.
        /// </summary>
        void Enter(Blackboard blackboard);

        /// <summary>
        /// Called every frame while this state is active.
        /// This is where the state's logic is executed.
        /// </summary>
        void Update(Blackboard blackboard);

        /// <summary>
        /// Called once when the state machine transitions out of this state.
        /// Use for cleanup.
        /// </summary>
        void Exit(Blackboard blackboard);
    }
}