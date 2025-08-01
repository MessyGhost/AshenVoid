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
        // The blackboard is now passed during evaluation, making nodes fully stateless.
        public abstract NodeState Evaluate(Blackboard blackboard);
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
        public override NodeState Evaluate(Blackboard blackboard)
        {
            foreach (var node in Children)
            {
                switch (node.Evaluate(blackboard))
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
        public override NodeState Evaluate(Blackboard blackboard)
        {
            foreach (var node in Children)
            {
                switch (node.Evaluate(blackboard))
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
        private readonly Func<Blackboard, NodeState> _action;
        public ActionNode(Func<Blackboard, NodeState> action) => _action = action;
        public override NodeState Evaluate(Blackboard blackboard) => _action(blackboard);
    }

    public class ConditionNode : Node
    {
        private readonly Func<Blackboard, bool> _condition;
        public ConditionNode(Func<Blackboard, bool> condition) => _condition = condition;
        public override NodeState Evaluate(Blackboard blackboard) => _condition(blackboard) ? NodeState.Success : NodeState.Failure;
    }

    public class WaitNode : Node
    {
        private readonly float _duration;
        // Timer is now stored on the blackboard to make the node stateless.
        private readonly string _timerKey;

        public WaitNode(float duration) 
        {
            _duration = duration;
            // Use a unique key for each wait node instance to avoid conflicts.
            _timerKey = $"WaitNode_{Guid.NewGuid()}";
        }

        public override NodeState Evaluate(Blackboard blackboard)
        {
            float timer = blackboard.Get<float>(_timerKey, _duration);

            timer -= (float)blackboard.Get<GameTime>(BlackboardKeys.GameTime).ElapsedGameTime.TotalSeconds;

            if (timer <= 0)
            {
                blackboard.Remove(_timerKey); // Clean up the timer from the blackboard
                return NodeState.Success;
            }
            
            blackboard.Set(_timerKey, timer);
            return NodeState.Running;
        }
    }

    #region New Reusable Action Nodes

    public class SetIntentNode<T> : Node where T : class, IIntent
    {
        private readonly string _intentKey;
        private readonly T _intent;

        public SetIntentNode(string intentKey, T intent)
        {
            _intentKey = intentKey;
            _intent = intent;
        }

        public override NodeState Evaluate(Blackboard blackboard)
        {
            blackboard.Set(_intentKey, _intent);
            return NodeState.Success;
        }
    }

    public class SetMovementIntentNode : SetIntentNode<IMovementIntent>
    {
        public SetMovementIntentNode(IMovementIntent intent) : base(BlackboardKeys.MovementIntent, intent) { }
    }
    
    public class SetAttackIntentNode : SetIntentNode<IAttackIntent>
    {
        public SetAttackIntentNode(IAttackIntent intent) : base(BlackboardKeys.AttackIntent, intent) { }
    }

    public class SetBlackboardValueNode<T> : Node
    {
        private readonly string _key;
        private readonly T _value;

        public SetBlackboardValueNode(string key, T value)
        {
            _key = key;
            _value = value;
        }

        public override NodeState Evaluate(Blackboard blackboard)
        {
            blackboard.Set(_key, _value);
            return NodeState.Success;
        }
    }

    #endregion
}