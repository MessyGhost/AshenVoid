using System;

namespace AshenVoid.Core.Sequencing
{
    public abstract class SequenceNode
    {
        public bool IsFinished { get; protected set; }
        public abstract void Update(float deltaTime);
    }

    public class ActionNode : SequenceNode
    {
        private readonly Action _action;

        public ActionNode(Action action)
        {
            _action = action;
            IsFinished = false;
        }

        public override void Update(float deltaTime)
        {
            if (IsFinished) return;
            _action();
            IsFinished = true;
        }
    }

    public class WaitNode : SequenceNode
    {
        private float _duration;

        public WaitNode(float duration)
        {
            _duration = duration;
        }

        public override void Update(float deltaTime)
        {
            if (IsFinished) return;
            _duration -= deltaTime;
            if (_duration <= 0)
            {
                IsFinished = true;
            }
        }
    }
}