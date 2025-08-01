using AshenVoid.Core.ECS.AI;
using AshenVoid.Core.ECS.BehaviorTree;

namespace AshenVoid.Core.ECS.FSM
{
    /// <summary>
    /// Defines a state within the FSM. Each state is now responsible for providing a behavior tree
    /// that dictates the AI's actions, and for checking conditions that trigger a transition to another state.
    /// </summary>
    public interface IState
    {
        /// <summary>
        /// Called once when the state machine enters this state.
        /// Use this to set up initial conditions or one-time actions for the state.
        /// </summary>
        void Enter(Blackboard blackboard);

        /// <summary>
        /// Called once when the state machine leaves this state.
        /// Use this to clean up any data or reset conditions.
        /// </summary>
        void Exit(Blackboard blackboard);

        /// <summary>
        /// Constructs and returns the behavior tree that governs the AI's logic within this state.
        /// This method is called when the state is entered.
        /// </summary>
        /// <returns>The root node of the behavior tree.</returns>
        Node BuildBehaviorTree(Blackboard blackboard);

        /// <summary>
        /// Called every frame to check if a transition to a different state should occur.
        /// </summary>
        /// <param name="blackboard">The shared data context.</param>
        /// <returns>The next state to transition to, or null to remain in the current state.</returns>
        IState CheckTransitions(Blackboard blackboard);
    }
}