using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;

namespace AshenVoid.Core.BehaviorTree
{
    /// <summary>
    /// 行为树节点执行状态
    /// </summary>
    public enum NodeState
    {
        Running,
        Success,
        Failure
    }

    /// <summary>
    /// 行为树基类
    /// </summary>
    public abstract class Node
    {
        /// <summary>
        /// 重置节点状态（用于循环执行）
        /// </summary>
        public virtual void Reset() { }

        /// <summary>
        /// 执行节点逻辑
        /// </summary>
        /// <returns>节点执行结果</returns>
        public abstract NodeState Evaluate();
    }

    /// <summary>
    /// 基本动作节点
    /// </summary>
    public class ActionNode : Node
    {
        private readonly Func<NodeState> _action;

        public ActionNode(Func<NodeState> action)
        {
            _action = action ?? throw new ArgumentNullException(nameof(action));
        }

        public override NodeState Evaluate() => _action();
    }

    /// <summary>
    /// 序列节点（顺序执行子节点）
    /// </summary>
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

    /// <summary>
    /// 回退节点（选择第一个成功的子节点）
    /// </summary>
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

    /// <summary>
    /// 条件节点（封装条件判断）
    /// </summary>
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

    /// <summary>
    /// 一次性执行节点
    /// </summary>
    public class OnceNode : Node
    {
        private readonly Func<NodeState> _action;
        private bool _executed;
        private readonly bool _alwaysSuccess;

        /// <param name="action">要执行的动作</param>
        /// <param name="alwaysSuccess">是否始终返回成功</param>
        public OnceNode(Func<NodeState> action, bool alwaysSuccess = true)
        {
            _action = action ?? throw new ArgumentNullException(nameof(action));
            _alwaysSuccess = alwaysSuccess;
        }

        public override NodeState Evaluate()
        {
            if (!_executed)
            {
                _executed = true;
                var result = _action();

                if (_alwaysSuccess)
                    return NodeState.Success;

                return result;
            }
            return NodeState.Success;
        }

        public override void Reset()
        {
            _executed = false;
            base.Reset();
        }
    }

    /// <summary>
    /// 重复执行节点
    /// </summary>
    public class RepeatNode : Node
    {
        private readonly Node _child;
        private readonly int _count;
        private int _currentIteration;

        /// <param name="child">要重复的节点</param>
        /// <param name="count">重复次数（-1表示无限）</param>
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

    /// <summary>
    /// 每隔固定时间执行一次子节点，支持指定执行次数（-1 表示无限循环）
    /// </summary>
    public class IntervalNode : Node
    {
        private readonly Node _child;
        private readonly float _interval;
        private float _elapsed;
        private bool _executingChild;
        private int _currentCount;
        private readonly int _totalCount; // -1 表示无限循环

        public IntervalNode(Node child, float intervalSeconds, int count = -1)
        {
            _child = child ?? throw new ArgumentNullException(nameof(child));
            _interval = intervalSeconds;
            _totalCount = count;
        }

        public override NodeState Evaluate()
        {
            if (_executingChild)
            {
                var result = _child.Evaluate();

                if (result != NodeState.Running)
                {
                    _executingChild = false;
                    _elapsed = 0f;
                    _child.Reset();

                    // 如果有执行次数限制，检查是否已达到上限
                    if (_totalCount != -1)
                    {
                        _currentCount++;
                        if (_currentCount >= _totalCount)
                        {
                            return NodeState.Success;
                        }
                    }
                }

                return result;
            }
            else
            {
                _elapsed += (float)Main.gameTimeCache.ElapsedGameTime.TotalSeconds;

                if (_elapsed >= _interval)
                {
                    _executingChild = true;
                    _elapsed = 0f;

                    var result = _child.Evaluate();

                    if (result != NodeState.Running)
                    {
                        _executingChild = false;
                        _elapsed = 0f;
                        _child.Reset();

                        if (_totalCount != -1)
                        {
                            _currentCount++;
                            if (_currentCount >= _totalCount)
                            {
                                return NodeState.Success;
                            }
                        }

                        return result;
                    }

                    return NodeState.Running;
                }

                return NodeState.Running;
            }
        }

        public override void Reset()
        {
            _elapsed = 0f;
            _executingChild = false;
            _currentCount = 0;
            _child.Reset();
            base.Reset();
        }
    }
    /// <summary>
    /// 超时保护节点
    /// </summary>
    public class DoUntilNode : Node
    {
        private readonly Node _child;
        private readonly float _timeout;
        private float _elapsed;

        public DoUntilNode(Node child, float timeoutSeconds)
        {
            _child = child ?? throw new ArgumentNullException(nameof(child));
            _timeout = timeoutSeconds;
        }

        public override NodeState Evaluate()
        {
            _elapsed += (float)Main.gameTimeCache.ElapsedGameTime.TotalSeconds;

            if (_elapsed >= _timeout)
            {
                _elapsed = 0f; // 可选：是否允许重复计时？视需求而定
                return NodeState.Success;
            }

            return _child.Evaluate();
        }

        public override void Reset()
        {
            _elapsed = 0f;
            _child.Reset();
            base.Reset();
        }
    }

    /// <summary>
    /// 并行执行节点
    /// </summary>
    public class ParallelNode : Node
    {
        private readonly List<Node> _children = new List<Node>();

        public ParallelNode(params Node[] children)
        {
            _children.AddRange(children);
        }

        public override NodeState Evaluate()
        {
            bool hasRunning = false;
            bool hasFailure = false;

            foreach (var child in _children)
            {
                var state = child.Evaluate();

                if (state == NodeState.Failure)
                {
                    hasFailure = true;
                }
                else if (state == NodeState.Running)
                {
                    hasRunning = true;
                }
            }

            if (hasFailure)
                return NodeState.Failure;

            return hasRunning ? NodeState.Running : NodeState.Success;
        }

        public override void Reset()
        {
            foreach (var child in _children)
                child.Reset();
            base.Reset();
        }
    }

    /// <summary>
    /// 随机选择器节点
    /// </summary>
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

    /// <summary>
    /// 节点构造器
    /// </summary>
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

        public static RepeatNode Repeat(Node node, int count = -1) =>
            new RepeatNode(node, count);

        public static IntervalNode Interval(Node node, float seconds, int count = -1) =>
            new IntervalNode(node, seconds, count);

        public static OnceNode Once(Func<NodeState> action, bool alwaysSuccess = true) =>
            new OnceNode(action, alwaysSuccess);

        public static DoUntilNode DoUntil(Node node, float timeoutSeconds) =>
            new DoUntilNode(node, timeoutSeconds);

        public static RandomSelectorNode Random(params Node[] nodes) =>
            new RandomSelectorNode(nodes);
    }
}