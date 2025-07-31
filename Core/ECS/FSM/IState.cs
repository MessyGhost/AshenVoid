using AshenVoid.Core.ECS;

namespace AshenVoid.Core.ECS.FSM
{
    /// <summary>
    /// Defines a state for a Finite State Machine (FSM).
    /// Each state represents a major phase or behavior of an NPC.
    /// </summary>
    public interface IState
    {
        /// <summary>
        /// Called once when the state machine transitions into this state.
        /// Use for initialization and setting up the state's behavior tree.
        /// </summary>
        /// <param name="ai">The AI component that owns this state machine.</param>
        void Enter(AIComponent ai);

        /// <summary>
        /// Called every frame while this state is active.
        /// This is where the state's logic, such as updating its behavior tree, is executed.
        /// </summary>
        void Update();

        /// <summary>
        /// Called once when the state machine transitions out of this state.
        /// Use for cleanup, such as stopping sounds or resetting variables.
        /// </summary>
        void Exit();
    }
}