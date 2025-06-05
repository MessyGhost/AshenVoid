using System;
using System.Collections.Generic;
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
        private List<Node> _children = new List<Node>();
        private int currentChildIndex = 0;

        public FallbackNode(params Node[] children)
        {
            _children.AddRange(children);
        }

        public override NodeState Evaluate()
        {
            for (; currentChildIndex < _children.Count; currentChildIndex++)
            {
                var child = _children[currentChildIndex];
                var state = child.Evaluate();

                if (state != NodeState.Failure)
                {
                    currentChildIndex = 0;
                    return state;
                }
            }

            currentChildIndex = 0;
            return NodeState.Failure;
        }

        public override void Reset()
        {
            currentChildIndex = 0;
            foreach (var child in _children)
                child.Reset();
            base.Reset();
        }
    }

    public class ParallelNode : Node
    {
        private List<Node> _children = new List<Node>();

        public ParallelNode(params Node[] children)
        {
            _children.AddRange(children);
        }

        public override NodeState Evaluate()
        {
            bool hasRunning = false;

            foreach (var child in _children)
            {
                var state = child.Evaluate();

                if (state == NodeState.Failure)
                    return NodeState.Failure;

                if (state == NodeState.Running)
                    hasRunning = true;
            }

            return hasRunning ? NodeState.Running : NodeState.Success;
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
        private Func<NodeState> _action;
        private bool _executed = false;

        public OnceNode(Func<NodeState> action)
        {
            _action = action;
        }

        public override NodeState Evaluate()
        {
            if (!_executed)
            {
                _executed = true;
                // Main.NewText("_executed");
                return _action();
            }
            return NodeState.Success;
        }

        public override void Reset()
        {
            _executed = false;
            base.Reset();
        }
    }
    public class DoUntilNode : Node
    {
        private readonly Node _child;
        private readonly Func<bool> _condition;

        public DoUntilNode(Node child, Func<bool> condition)
        {
            _child = child ?? throw new ArgumentNullException(nameof(child));
            _condition = condition ?? throw new ArgumentNullException(nameof(condition));
        }

        public override NodeState Evaluate()
        {
            if (_condition())
            {
                _child.Reset();
                return NodeState.Success;
            }

            var result = _child.Evaluate();
            if (result == NodeState.Failure)
                return NodeState.Failure;

            if (result == NodeState.Success)
                _child.Reset();

            return NodeState.Running;
        }

        public override void Reset()
        {
            _child.Reset();
            base.Reset();
        }
    }

    public class DoSecondsNode : Node
    {
        private readonly Node _child;
        private readonly float _timeoutSeconds;

        private float _elapsedTime;

        public DoSecondsNode(Node child, float timeoutSeconds)
        {
            _child = child ?? throw new ArgumentNullException(nameof(child));
            _timeoutSeconds = timeoutSeconds;
        }

        public override NodeState Evaluate()
        {
            if (_elapsedTime >= _timeoutSeconds)
            {
                _child.Reset();
                return NodeState.Success;
            }

            _elapsedTime += (float)Main.gameTimeCache.ElapsedGameTime.TotalSeconds;
            var result = _child.Evaluate();

            if (result == NodeState.Failure)
                return NodeState.Failure;

            return NodeState.Running;
        }

        public override void Reset()
        {
            _elapsedTime = 0f;
            _child.Reset();
            base.Reset();
        }
    }

    public class DoWhenNode : Node
    {
        private readonly Func<bool> _condition;
        private readonly Node _child;

        public DoWhenNode(Func<bool> condition, Node child)
        {
            _condition = condition ?? throw new ArgumentNullException(nameof(condition));
            _child = child ?? throw new ArgumentNullException(nameof(child));
        }

        public override NodeState Evaluate()
        {
            if (_condition())
                return _child.Evaluate();

            _child.Reset();
            return NodeState.Failure;
        }

        public override void Reset()
        {
            _child.Reset();
            base.Reset();
        }
    }

    public class RepeatNode : Node
    {
        private Node _child;
        private int _count;
        private int _currentIteration;

        public RepeatNode(Node child, int count = -1)
        {
            _child = child;
            _count = count;
        }

        public override NodeState Evaluate()
        {
            while (_count < 0 || _currentIteration < _count)
            {
                var result = _child.Evaluate();

                if (result == NodeState.Running)
                    return NodeState.Running;

                if (result == NodeState.Failure)
                    return NodeState.Failure;

                _currentIteration++;
                _child.Reset();
            }

            return NodeState.Success;
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
        private readonly Random _random = new Random();

        public RandomSelectorNode(params Node[] children)
        {
            _children.AddRange(children);
        }

        public override NodeState Evaluate()
        {
            int index = _random.Next(_children.Count);
            return _children[index].Evaluate();
        }

        public override void Reset()
        {
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

        public static ConditionNode Condition(Func<bool> condition) =>
            new ConditionNode(condition);

        public static ActionNode Do(Func<NodeState> action) =>
            new ActionNode(action);
        public static ActionNode Do(Action action) =>
            new ActionNode(() => { action(); return NodeState.Success; });

        public static RepeatNode Repeat(Node node, int count = -1) =>
            new RepeatNode(node, count);

        public static OnceNode Once(Func<NodeState> action) =>
            new OnceNode(action);

        public static OnceNode Once(Action action) =>
            new OnceNode(() => { action(); return NodeState.Success; });

        public static RepeatNode Interval(Node node, float seconds, int count = -1) =>
            new RepeatNode(new SequenceNode(node, new WaitFramesNode((int)(seconds * 60))), count);

        public static WaitUntilNode WaitUntil(Func<bool> condition) =>
            new WaitUntilNode(condition);

        public static WaitFramesNode WaitFrames(int frames) =>
            new WaitFramesNode(frames);

        public static DoUntilNode DoUntil(Node node, Func<bool> condition) =>
            new DoUntilNode(node, condition);

        public static DoSecondsNode DoSeconds(Node node, float timeoutSeconds) =>
            new DoSecondsNode(node, timeoutSeconds);

        public static DoWhenNode DoWhen(Func<bool> condition, Node node) =>
            new DoWhenNode(condition, node);
    }
}