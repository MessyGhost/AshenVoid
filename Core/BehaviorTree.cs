using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;

namespace AshenVoid.Core.BehaviorTree
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

    public class SequenceNode : Node
    {
        private readonly List<Node> _children = new List<Node>();
        private int _currentChildIndex;

        public SequenceNode(params Node[] children)
        {
            _children.AddRange(children);
        }

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
                    _currentChildIndex = 0;
                    return NodeState.Failure;
                }

                _currentChildIndex++;
            }

            _currentChildIndex = 0;
            return NodeState.Success;
        }

        public override void Reset()
        {
            _currentChildIndex = 0;
            foreach (var child in _children)
                child.Reset();
            base.Reset();
        }
    }


    public class FallbackNode : Node
    {
        private readonly List<Node> _children = new List<Node>();
        private int _currentChildIndex;

        public FallbackNode(params Node[] children)
        {
            _children.AddRange(children);
        }

        public override NodeState Evaluate()
        {
            for (; _currentChildIndex < _children.Count; _currentChildIndex++)
            {
                var result = _children[_currentChildIndex].Evaluate();

                if (result != NodeState.Failure)
                {
                    _currentChildIndex = 0;
                    return result;
                }
            }

            _currentChildIndex = 0;
            return NodeState.Failure;
        }

        public override void Reset()
        {
            _currentChildIndex = 0;
            foreach (var child in _children)
                child.Reset();
            base.Reset();
        }
    }
    public class ParallelNode : Node
    {
        private readonly List<Node> _children = new List<Node>();

        public ParallelNode(params Node[] children)
        {
            _children.AddRange(children);
        }

        public override NodeState Evaluate()
        {
            bool allSuccess = true;
            bool anyRunning = false;

            foreach (var child in _children)
            {
                var state = child.Evaluate();
                if (state == NodeState.Failure)
                {
                    return NodeState.Failure;
                }
                if (state == NodeState.Running)
                {
                    anyRunning = true;
                }
                if (state != NodeState.Success)
                {
                    allSuccess = false;
                }
            }

            if (anyRunning)
            {
                return NodeState.Running;
            }

            return allSuccess ? NodeState.Success : NodeState.Failure;
        }

        public override void Reset()
        {
            foreach (var child in _children)
                child.Reset();
            base.Reset();
        }
    }

    public class ConditionNode : Node
    {
        private readonly Func<bool> _condition;

        public ConditionNode(Func<bool> condition)
        {
            _condition = condition ?? throw new ArgumentNullException(nameof(condition));
        }

        public override NodeState Evaluate() =>
            _condition() ? NodeState.Success : NodeState.Failure;
    }

    public class OnceNode : Node
    {
        private Node _child;
        private bool _hasBeenCalled = false;

        public OnceNode(Node child)
        {
            _child = child;
        }

        public override NodeState Evaluate()
        {
            if (_hasBeenCalled)
            {
                return NodeState.Success;
            }

            var state = _child.Evaluate();

            if (state != NodeState.Running)
            {
                _hasBeenCalled = true;
            }

            return state;
        }
        public override void Reset()
        {
            _hasBeenCalled = false;
            _child.Reset();
            base.Reset();
        }
    }

    public class RepeatNode : Node
    {
        private readonly Node _child;
        private readonly int _count;
        private int _currentIteration;
        public RepeatNode(Node child, int count = -1)
        {
            _child = child ?? throw new ArgumentNullException(nameof(child));
            _count = count;
        }

        public override NodeState Evaluate()
        {
            if (_count >= 0 && _currentIteration >= _count)
                return NodeState.Success;

            var result = _child.Evaluate();

            if (result == NodeState.Running)
                return NodeState.Running;

            if (result == NodeState.Failure)
                return NodeState.Failure;

            _currentIteration++;
            _child.Reset();

            return _count < 0 || _currentIteration < _count
                ? NodeState.Running
                : NodeState.Success;
        }

        public override void Reset()
        {
            _currentIteration = 0;
            _child.Reset();
            base.Reset();
        }
    }

    public class RandomSelectorNode : Node
    {
        private readonly List<Node> _children = new List<Node>();
        private readonly List<int> _weights = new List<int>();
        private readonly Random _random = new Random();
        private bool _useWeights;
        private int? _currentChildIndex = null;

        // 构造函数 1：等概率选择
        public RandomSelectorNode(params Node[] children)
        {
            _children.AddRange(children);
        }

        // 构造函数 2：带权重选择
        public RandomSelectorNode(params (Node node, int weight)[] weightedNodes)
        {
            foreach (var (node, weight) in weightedNodes)
            {
                _children.Add(node);
                _weights.Add(weight);
            }
            _useWeights = true;
        }

        public override NodeState Evaluate()
        {
            if (_children.Count == 0)
                return NodeState.Failure;

            // 如果尚未选择子节点，则根据权重或随机选择一个
            if (!_currentChildIndex.HasValue)
            {
                if (!_useWeights || _weights.All(w => w == 1))
                {
                    // 等概率选择
                    _currentChildIndex = _random.Next(_children.Count);
                }
                else
                {
                    // 按权重选择
                    int totalWeight = _weights.Sum();
                    if (totalWeight <= 0)
                        return NodeState.Failure;

                    int selectedWeight = _random.Next(totalWeight);
                    int cumulative = 0;

                    for (int i = 0; i < _children.Count; i++)
                    {
                        cumulative += _weights[i];
                        if (selectedWeight < cumulative)
                        {
                            _currentChildIndex = i;
                            break;
                        }
                    }

                    // fallback: 如果未找到（理论上不会发生）
                    if (!_currentChildIndex.HasValue)
                        _currentChildIndex = _children.Count - 1;
                }
            }

            // 执行当前选中的子节点
            var result = _children[_currentChildIndex.Value].Evaluate();

            // 如果子节点完成，则重置以备下次重新选择
            if (result != NodeState.Running)
            {
                _currentChildIndex = null;
            }

            return result;
        }

        public override void Reset()
        {
            _currentChildIndex = null;
            foreach (var child in _children)
                child.Reset();
            base.Reset();
        }
    }

    public class WaitUntilNode : Node
    {
        private Func<bool> _condition;
        private bool _hasStarted = false;

        public WaitUntilNode(Func<bool> condition)
        {
            _condition = condition;
        }

        public override NodeState Evaluate()
        {
            _hasStarted = true;
            return _condition() ? NodeState.Success : NodeState.Running;
        }

        public override void Reset()
        {
            _hasStarted = false;
            base.Reset();
        }
    }

    public class WaitFramesNode : Node
    {
        private readonly int _totalFrames;
        private int _framesRemaining;

        public WaitFramesNode(int frames)
        {
            _totalFrames = frames;
            _framesRemaining = frames;
        }

        public override NodeState Evaluate()
        {
            if (_framesRemaining <= 0)
            {
                return NodeState.Success;
            }

            _framesRemaining--;
            return NodeState.Running;
        }

        public override void Reset()
        {
            _framesRemaining = _totalFrames;
            base.Reset();
        }
    }
    public static class NodeBuilder
    {
        public static SequenceNode Sequence(params Node[] nodes) =>
            new SequenceNode(nodes);

        public static FallbackNode Fallback(params Node[] nodes) =>
            new FallbackNode(nodes);

        public static ParallelNode Parallel(params Node[] nodes) =>
            new ParallelNode(nodes);

        public static RandomSelectorNode Random(params Node[] nodes) =>
            new RandomSelectorNode(nodes);

        public static RandomSelectorNode Weighted(params (Node node, int weight)[] weightedNodes) =>
            new RandomSelectorNode(weightedNodes);
        public static RepeatNode Repeat(Node node, int count = -1) =>
            new RepeatNode(node, count);
        public static ConditionNode Condition(Func<bool> condition) =>
            new ConditionNode(condition);

        public static ActionNode Do(Func<NodeState> action) =>
            new ActionNode(action);

        public static ActionNode Do(Action action) =>
            new ActionNode(() => { action(); return NodeState.Success; });

        public static OnceNode Once(Node node) =>
            new OnceNode(node);

        public static OnceNode Once(Func<NodeState> action) =>
            new OnceNode(Do(action));

        public static OnceNode Once(Action action) =>
            new OnceNode(Do(() => { action(); return NodeState.Success; }));

        public static WaitUntilNode WaitUntil(Func<bool> condition) =>
            new WaitUntilNode(condition);

        public static WaitFramesNode WaitFrames(int frames) =>
            new WaitFramesNode(frames);

        public static WaitFramesNode WaitSeconds(float seconds) =>
            new WaitFramesNode((int)(seconds * 60));

        public static RepeatNode Interval(Node node, float seconds, int count = -1) =>
            Repeat(Sequence(node, WaitSeconds(seconds)), count);

        public static RepeatNode Interval(Node mainNode, Node intervalNode, int count = -1) =>
            Repeat(Sequence(mainNode, intervalNode), count);

        public static RepeatNode DoUntil(Node node, Func<bool> condition) =>
            Repeat(Sequence(Condition(condition), node));

        public static ParallelNode DoSeconds(Node node, float timeoutSeconds) =>
            Parallel(node, WaitFrames((int)(timeoutSeconds * 60)));
    }
}