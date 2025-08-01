using System;
using System.Collections.Generic;

namespace AshenVoid.Core.ECS.BehaviorTree
{
    public enum NodeStatus
    {
        Running,
        Success,
        Failure
    }

    public abstract class Node
    {
        public abstract NodeStatus Tick(int entityId, EcsWorld world);
    }

    public abstract class CompositeNode : Node
    {
        protected readonly List<Node> Children = new();

        public CompositeNode Add(Node child)
        {
            Children.Add(child);
            return this;
        }
    }

    public class Selector : CompositeNode
    {
        public override NodeStatus Tick(int entityId, EcsWorld world)
        {
            foreach (var child in Children)
            {
                var status = child.Tick(entityId, world);
                if (status != NodeStatus.Failure)
                {
                    return status;
                }
            }
            return NodeStatus.Failure;
        }
    }

    public class Sequence : CompositeNode
    {
        public override NodeStatus Tick(int entityId, EcsWorld world)
        {
            foreach (var child in Children)
            {
                var status = child.Tick(entityId, world);
                if (status != NodeStatus.Success)
                {
                    return status;
                }
            }
            return NodeStatus.Success;
        }
    }

    public class ActionNode : Node
    {
        private readonly Func<int, EcsWorld, NodeStatus> _action;

        public ActionNode(Func<int, EcsWorld, NodeStatus> action)
        {
            _action = action;
        }

        public override NodeStatus Tick(int entityId, EcsWorld world)
        {
            return _action(entityId, world);
        }
    }
}