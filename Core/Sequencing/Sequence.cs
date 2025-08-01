using System;
using System.Collections.Generic;

namespace AshenVoid.Core.Sequencing
{
    public class Sequence
    {
        private readonly Queue<SequenceNode> _nodes = new();
        private SequenceNode _currentNode;

        public bool IsFinished { get; private set; }

        public Sequence Then(Action action)
        {
            _nodes.Enqueue(new ActionNode(action));
            return this;
        }

        public Sequence WaitFor(float duration)
        {
            _nodes.Enqueue(new WaitNode(duration));
            return this;
        }

        public void Update(float deltaTime)
        {
            if (IsFinished) return;

            if (_currentNode == null)
            {
                if (_nodes.Count > 0)
                {
                    _currentNode = _nodes.Dequeue();
                }
                else
                {
                    IsFinished = true;
                    return;
                }
            }

            _currentNode.Update(deltaTime);

            if (_currentNode.IsFinished)
            {
                _currentNode = null;
            }
        }
    }
}