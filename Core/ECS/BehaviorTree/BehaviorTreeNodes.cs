using AshenVoid.Core.ECS.AI;
using AshenVoid.Core.ECS.Intents;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;

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
        protected Blackboard Blackboard { get; private set; }

        public void SetBlackboard(Blackboard blackboard)
        {
            Blackboard = blackboard;
            foreach (var child in GetChildren())
            {
                child.SetBlackboard(blackboard);
            }
        }

        public abstract NodeState Evaluate();
        public virtual IEnumerable<Node> GetChildren() => new List<Node>();
    }

    public abstract class CompositeNode : Node
    {
        protected readonly List<Node> Children = new List<Node>();
        public CompositeNode(params Node[] children)
        {
            Children.AddRange(children);
        }
        public override IEnumerable<Node> GetChildren() => Children;
    }

    public class SequenceNode : CompositeNode
    {
        public SequenceNode(params Node[] children) : base(children) { }
        public override NodeState Evaluate()
        {
            foreach (var node in Children)
            {
                switch (node.Evaluate())
                {
                    case NodeState.Failure:
                        return NodeState.Failure;
                    case NodeState.Running:
                        return NodeState.Running;
                    case NodeState.Success:
                        continue;
                }
            }
            return NodeState.Success;
        }
    }

    public class FallbackNode : CompositeNode
    {
        public FallbackNode(params Node[] children) : base(children) { }
        public override NodeState Evaluate()
        {
            foreach (var node in Children)
            {
                switch (node.Evaluate())
                {
                    case NodeState.Running:
                        return NodeState.Running;
                    case NodeState.Success:
                        return NodeState.Success;
                    case NodeState.Failure:
                        continue;
                }
            }
            return NodeState.Failure;
        }
    }

    public class ActionNode : Node
    {
        private readonly Func<NodeState> _action;
        public ActionNode(Func<NodeState> action) => _action = action;
        public override NodeState Evaluate() => _action();
    }

    public class ConditionNode : Node
    {
        private readonly Func<bool> _condition;
        public ConditionNode(Func<bool> condition) => _condition = condition;
        public override NodeState Evaluate() => _condition() ? NodeState.Success : NodeState.Failure;
    }

    public class WaitNode : Node
    {
        private readonly float _duration;
        private float _timer;
        public WaitNode(float duration) => _duration = duration;

        public override NodeState Evaluate()
        {
            if (_timer == 0)
            {
                _timer = _duration;
            }

            _timer -= (float)Blackboard.Get<GameTime>(BlackboardKeys.GameTime).ElapsedGameTime.TotalSeconds;

            if (_timer <= 0)
            {
                _timer = 0;
                return NodeState.Success;
            }
            return NodeState.Running;
        }
    }

    #region New Reusable Action Nodes

    /// <summary>
    /// Sets a specific intent on the blackboard.
    /// </summary>
    public class SetIntentNode<T> : Node where T : class, IIntent
    {
        private readonly string _intentKey;
        private readonly T _intent;

        public SetIntentNode(string intentKey, T intent)
        {
            _intentKey = intentKey;
            _intent = intent;
        }

        public override NodeState Evaluate()
        {
            Blackboard.Set(_intentKey, _intent);
            return NodeState.Success;
        }
    }

    /// <summary>
    /// A more specialized node for setting movement intents.
    /// </summary>
    public class SetMovementIntentNode : SetIntentNode<IMovementIntent>
    {
        public SetMovementIntentNode(IMovementIntent intent) : base(BlackboardKeys.MovementIntent, intent) { }
    }
    
    /// <summary>
    /// A more specialized node for setting attack intents.
    /// </summary>
    public class SetAttackIntentNode : SetIntentNode<IAttackIntent>
    {
        public SetAttackIntentNode(IAttackIntent intent) : base(BlackboardKeys.AttackIntent, intent) { }
    }

    /// <summary>
    /// Sets a value on the blackboard.
    /// </summary>
    public class SetBlackboardValueNode<T> : Node
    {
        private readonly string _key;
        private readonly T _value;

        public SetBlackboardValueNode(string key, T value)
        {
            _key = key;
            _value = value;
        }

        public override NodeState Evaluate()
        {
            Blackboard.Set(_key, _value);
            return NodeState.Success;
        }
    }

    #endregion
}