using System;
using System.Collections.Generic;
using System.Linq;

namespace AshenVoid.Core.ECS.BehaviorTree
{
    public enum NodeState
    {
        Running,
        Success,
        Failure
    }

    public abstract class Node
    {
        public virtual void Reset() { }
        public abstract NodeState Evaluate();
    }

    public class ActionNode : Node
    {
        private readonly Func<NodeState> _action;
        public ActionNode(Func<NodeState> action)
        {
            _action = action ?? throw new ArgumentNullException(nameof(action));
        }
        public override NodeState Evaluate() => _action();
    }

    public class ConditionNode : Node
    {
        private readonly Func<bool> _condition;
        public ConditionNode(Func<bool> condition)
        {
            _condition = condition ?? throw new ArgumentNullException(nameof(condition));
        }
        public override NodeState Evaluate() => _condition() ? NodeState.Success : NodeState.Failure;
    }

    public class SequenceNode : Node
    {
        private readonly List<Node> _children;
        private int _currentChildIndex = 0;

        public SequenceNode(params Node[] children) { _children = children.ToList(); }

        public override NodeState Evaluate()
        {
            while (_currentChildIndex < _children.Count)
            {
                var child = _children[_currentChildIndex];
                var result = child.Evaluate();

                if (result == NodeState.Running)
                    return NodeState.Running;

                if (result == NodeState.Failure)
                {
                    Reset();
                    return NodeState.Failure;
                }

                _currentChildIndex++;
            }

            Reset();
            return NodeState.Success;
        }

        public override void Reset()
        {
            _currentChildIndex = 0;
            foreach (var child in _children)
                child.Reset();
        }
    }

    public class FallbackNode : Node
    {
        private readonly List<Node> _children;
        private int _currentChildIndex = 0;

        public FallbackNode(params Node[] children) { _children = children.ToList(); }

        public override NodeState Evaluate()
        {
            while (_currentChildIndex < _children.Count)
            {
                var child = _children[_currentChildIndex];
                var result = child.Evaluate();

                if (result == NodeState.Running)
                    return NodeState.Running;

                if (result == NodeState.Success)
                {
                    Reset();
                    return NodeState.Success;
                }

                _currentChildIndex++;
            }

            Reset();
            return NodeState.Failure;
        }

        public override void Reset()
        {
            _currentChildIndex = 0;
            foreach (var child in _children)
                child.Reset();
        }
    }

    public class InverterNode : Node
    {
        private readonly Node _child;
        public InverterNode(Node child) { _child = child; }
        public override NodeState Evaluate()
        {
            var result = _child.Evaluate();
            switch (result)
            {
                case NodeState.Success: return NodeState.Failure;
                case NodeState.Failure: return NodeState.Success;
                default: return result;
            }
        }
        public override void Reset() => _child.Reset();
    }

    public class RandomSelectorNode : Node
    {
        private readonly List<Node> _children;
        private readonly List<int> _weights;
        private readonly Random _random = new Random();
        private readonly bool _useWeights;
        private int? _currentChildIndex;

        public RandomSelectorNode(params Node[] children)
        {
            _children = children.ToList();
            _useWeights = false;
        }

        public RandomSelectorNode(params (Node node, int weight)[] weightedNodes)
        {
            _children = new List<Node>();
            _weights = new List<int>();
            foreach (var (node, weight) in weightedNodes)
            {
                _children.Add(node);
                _weights.Add(weight);
            }
            _useWeights = true;
        }

        public override NodeState Evaluate()
        {
            if (!_currentChildIndex.HasValue)
            {
                _currentChildIndex = SelectChildIndex();
            }

            if (!_currentChildIndex.HasValue) return NodeState.Failure;

            var result = _children[_currentChildIndex.Value].Evaluate();
            if (result != NodeState.Running)
            {
                _currentChildIndex = null;
            }
            return result;
        }

        private int? SelectChildIndex()
        {
            if (_children.Count == 0) return null;

            if (!_useWeights || _weights.All(w => w <= 0))
            {
                return _random.Next(_children.Count);
            }

            int totalWeight = _weights.Sum();
            if (totalWeight <= 0) return null;

            int randomWeight = _random.Next(totalWeight);
            int cumulativeWeight = 0;
            for (int i = 0; i < _children.Count; i++)
            {
                cumulativeWeight += _weights[i];
                if (randomWeight < cumulativeWeight)
                {
                    return i;
                }
            }
            return _children.Count - 1;
        }

        public override void Reset()
        {
            _currentChildIndex = null;
            foreach (var child in _children)
                child.Reset();
        }
    }

    /// <summary>
    /// A non-blocking wait node. Returns Running until the specified duration has passed.
    /// </summary>
    public class WaitNode : Node
    {
        private readonly float _duration;
        private float _timer;

        public WaitNode(float duration)
        {
            _duration = duration;
            _timer = 0f;
        }

        public override NodeState Evaluate()
        {
            if (_timer >= _duration)
            {
                return NodeState.Success;
            }

            _timer += 1f / 60f; // Assuming 60 TPS
            return NodeState.Running;
        }

        public override void Reset()
        {
            _timer = 0f;
        }
    }

    public static class NodeBuilder
    {
        public static SequenceNode Sequence(params Node[] nodes) => new SequenceNode(nodes);
        public static FallbackNode Fallback(params Node[] nodes) => new FallbackNode(nodes);
        public static InverterNode Inverter(Node node) => new InverterNode(node);
        public static RandomSelectorNode Random(params Node[] nodes) => new RandomSelectorNode(nodes);
        public static RandomSelectorNode Weighted(params (Node node, int weight)[] weightedNodes) => new RandomSelectorNode(weightedNodes);
        public static ActionNode Do(Action action) => new ActionNode(() => { action(); return NodeState.Success; });
        public static WaitNode Wait(float seconds) => new WaitNode(seconds);
    }
}