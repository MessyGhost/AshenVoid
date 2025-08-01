using AshenVoid.Core.ECS.AI;
using AshenVoid.Core.ECS.Intents;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;

namespace AshenVoid.Core.ECS.BehaviorTree
{
    // ... (Node, NodeState, CompositeNode, SequenceNode, SelectorNode are unchanged) ...

    public abstract class Node
    {
        public abstract NodeState Evaluate(Blackboard blackboard);
    }

    public enum NodeState
    {
        Running,
        Success,
        Failure
    }

    public abstract class CompositeNode : Node
    {
        protected readonly List<Node> Children = new List<Node>();

        public CompositeNode(params Node[] children)
        {
            Children.AddRange(children);
        }
    }

    public class SequenceNode : CompositeNode
    {
        public SequenceNode(params Node[] children) : base(children) { }

        public override NodeState Evaluate(Blackboard blackboard)
        {
            foreach (var child in Children)
            {
                switch (child.Evaluate(blackboard))
                {
                    case NodeState.Running:
                        return NodeState.Running;
                    case NodeState.Failure:
                        return NodeState.Failure;
                    case NodeState.Success:
                        continue;
                }
            }
            return NodeState.Success;
        }
    }

    public class SelectorNode : CompositeNode
    {
        public SelectorNode(params Node[] children) : base(children) { }

        public override NodeState Evaluate(Blackboard blackboard)
        {
            foreach (var child in Children)
            {
                switch (child.Evaluate(blackboard))
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

        public ActionNode(Func<Blackboard, NodeState> action)
        {
            _action = action;
        }

        public override NodeState Evaluate(Blackboard blackboard)
        {
            return _action(blackboard);
        }
    }

    public class WaitNode : Node
    {
        private readonly float _duration;
        private float _startTime = -1;

        public WaitNode(float duration)
        {
            _duration = duration;
        }

        public override NodeState Evaluate(Blackboard blackboard)
        {
            // The concept of getting GameTime from the blackboard is now obsolete.
            // This needs a redesign, but for now, we'll use Main.gameTimeCache.
            var gameTime = Main.gameTimeCache;
            if (_startTime < 0)
            {
                _startTime = (float)gameTime.TotalGameTime.TotalSeconds;
            }

            if (gameTime.TotalGameTime.TotalSeconds - _startTime >= _duration)
            {
                _startTime = -1;
                return NodeState.Success;
            }

            return NodeState.Running;
        }
    }

    public class SetMovementIntentNode : Node
    {
        private readonly Func<Blackboard, IMovementIntent> _intentProvider;

        public SetMovementIntentNode(Func<Blackboard, IMovementIntent> intentProvider)
        {
            _intentProvider = intentProvider;
        }

        public override NodeState Evaluate(Blackboard blackboard)
        {
            // This is now obsolete. Intents should be handled by systems or states directly.
            // blackboard.Set(BlackboardKeys.MovementIntent, _intentProvider(blackboard));
            return NodeState.Success;
        }
    }

    public class SetAttackIntentNode : Node
    {
        private readonly Func<Blackboard, IAttackIntent> _intentProvider;

        public SetAttackIntentNode(Func<Blackboard, IAttackIntent> intentProvider)
        {
            _intentProvider = intentProvider;
        }

        public override NodeState Evaluate(Blackboard blackboard)
        {
            // This is now obsolete.
            // blackboard.Set(BlackboardKeys.AttackIntent, _intentProvider(blackboard));
            return NodeState.Success;
        }
    }
}