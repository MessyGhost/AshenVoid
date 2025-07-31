using AshenVoid.Core.ECS;

namespace AshenVoid.Core.ECS.FSM
{
    /// <summary>
    /// Manages the states of an AI, handling transitions and updates.
    /// </summary>
    public class StateMachine
    {
        public IState CurrentState { get; private set; }
        private readonly AIComponent _owner;

        public StateMachine(AIComponent owner)
        {
            _owner = owner;
        }

        /// <summary>
        /// Transitions to a new state.
        /// </summary>
        /// <param name="newState">The state to transition to.</param>
        public void ChangeState(IState newState)
        {
            CurrentState?.Exit();
            CurrentState = newState;
            CurrentState?.Enter(_owner);
        }

        /// <summary>
        /// Updates the current state.
        /// </summary>
        public void Update()
        {
            CurrentState?.Update();
        }
    }
}