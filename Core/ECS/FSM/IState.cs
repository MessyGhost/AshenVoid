using AshenVoid.Core.ECS.AI;

namespace AshenVoid.Core.ECS.FSM
{
    /// <summary>
    /// Defines a state within the FSM. A state is now a simple object
    /// that primarily contains Enter and Exit logic.
    /// </summary>
    public interface IState
    {
        /// &lt;summary&gt;
        /// The behavior tree that governs this state's logic.
        /// &lt;/summary&gt;
        BehaviorTree.Node BehaviorTree { get; }
        /// <summary>
        /// Called once when the state machine enters this state.
        /// </summary>
        void Enter(int entityId, EcsWorld world);

        /// <summary>
        /// Called once when the state machine leaves this state.
        /// </summary>
        void Exit(int entityId, EcsWorld world);

        /// <summary>
        /// Called every frame to update the state's logic.
        /// </summary>
        void Update(int entityId, EcsWorld world);

        /// <summary>
        /// Called every frame to check for transitions to other states.
        /// </summary>
        /// <returns>The type of the next state, or null to remain.</returns>
        System.Type CheckTransitions(int entityId, EcsWorld world);
    }
}