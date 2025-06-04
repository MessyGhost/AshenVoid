using System;
using System.Collections.Generic;
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
        public virtual void Reset() { } // 用于重置节点状态
        public abstract NodeState Evaluate();
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

    public class SequenceNode : Node
    {
        private List<Node> _children = new List<Node>();
        private int currentChildIndex = 0;

        public SequenceNode(params Node[] children)
        {
            _children.AddRange(children);
        }

        public override NodeState Evaluate()
        {
            while (currentChildIndex < _children.Count)
            {
                var child = _children[currentChildIndex];
                var state = child.Evaluate();

                if (state == NodeState.Running)
                {
                    return NodeState.Running;
                }

                if (state == NodeState.Failure)
                {
                    currentChildIndex = 0; // 重置索引
                    return NodeState.Failure;
                }

                // 成功则继续下一个节点
                currentChildIndex++;
            }

            // 所有子节点执行完毕
            currentChildIndex = 0;
            return NodeState.Success;
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
        private Func<bool> _condition;

        public ConditionNode(Func<bool> condition)
        {
            _condition = condition;
        }

        public override NodeState Evaluate()
        {
            return _condition() ? NodeState.Success : NodeState.Failure;
        }
    }

    // 带状态保持的等待节点
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

    // 基于帧数的等待节点
    public class WaitFramesNode : Node
    {
        private int _framesRemaining;

        public WaitFramesNode(int frames)
        {
            _framesRemaining = frames;
        }

        public override NodeState Evaluate()
        {
            if (_framesRemaining <= 0)
                return NodeState.Success;

            _framesRemaining--;
            return NodeState.Running;
        }

        public override void Reset()
        {
            _framesRemaining = 0;
            base.Reset();
        }
    }

    // 动作节点
    public class ActionNode : Node
    {
        private Func<NodeState> _action;

        public ActionNode(Func<NodeState> action)
        {
            _action = action;
        }

        public override NodeState Evaluate()
        {
            return _action();
        }
    }

    // 装饰器节点：重复执行子节点
    public class RepeatNode : Node
    {
        private Node _child;
        private int _count;
        private int _currentIteration;

        public RepeatNode(Node child, int count)
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

    // 超时装饰器（防止死循环）
    public class TimeoutNode : Node
    {
        private Node _child;
        private float _timeout;
        private float _elapsed;

        public TimeoutNode(Node child, float timeout)
        {
            _child = child;
            _timeout = timeout;
        }

        public override NodeState Evaluate()
        {
            _elapsed += (float)Main.time;
            if (_elapsed >= _timeout)
                return NodeState.Failure;

            return _child.Evaluate();
        }

        public override void Reset()
        {
            _elapsed = 0;
            _child.Reset();
            base.Reset();
        }
    }

    // 随机选择器
    public class RandomSelectorNode : Node
    {
        private List<Node> _children = new List<Node>();
        private Random _rand = new Random();

        public RandomSelectorNode(params Node[] children)
        {
            _children.AddRange(children);
        }

        public override NodeState Evaluate()
        {
            int index = _rand.Next(_children.Count);
            return _children[index].Evaluate();
        }

        public override void Reset()
        {
            foreach (var child in _children)
                child.Reset();
            base.Reset();
        }
    }

    public static class BehaviorTreeBuilder
    {
        public static SequenceNode Sequence(params Node[] nodes) => new SequenceNode(nodes);
        public static ParallelNode Parallel(params Node[] nodes) => new ParallelNode(nodes);
        public static FallbackNode Fallback(params Node[] nodes) => new FallbackNode(nodes);
        public static ConditionNode Condition(Func<bool> condition) => new ConditionNode(condition);
        public static WaitUntilNode WaitUntil(Func<bool> condition) => new WaitUntilNode(condition);
        public static WaitFramesNode WaitFrames(int frames) => new WaitFramesNode(frames);
        public static ActionNode Action(Func<NodeState> action) => new ActionNode(action);
        public static RepeatNode Repeat(Node node, int count) => new RepeatNode(node, count);
        public static TimeoutNode Timeout(Node node, float seconds) => new TimeoutNode(node, seconds);
        public static RandomSelectorNode Random(params Node[] nodes) => new RandomSelectorNode(nodes);
    }
}