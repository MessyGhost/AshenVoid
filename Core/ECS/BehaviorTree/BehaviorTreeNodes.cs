using AshenVoid.Core.ECS.AI;
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

    // The concept of Intent nodes is obsolete in the new architecture.
    // Systems should react to component data changes or events.
    // These classes are left here as placeholders but are effectively empty.
    public class SetMovementIntentNode : Node
    {
        public SetMovementIntentNode(object intentProvider) { } // Dummy constructor
        public override NodeState Evaluate(Blackboard blackboard) => NodeState.Success;
    }

    public class SetAttackIntentNode : Node
    {
        public SetAttackIntentNode(object intentProvider) { } // Dummy constructor
        public override NodeState Evaluate(Blackboard blackboard) => NodeState.Success;
    }
}