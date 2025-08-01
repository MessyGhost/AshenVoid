using AshenVoid.Content.NPCs.NightmareCorruption.Configs;
using AshenVoid.Core.ECS.AI;
using System;
using System.Collections.Generic;

namespace AshenVoid.Core.ECS.BehaviorTree
{
    public class AIBehaviorFactory
    {
        private readonly Dictionary<string, Func<Node>> _behaviorTrees = new();
        private readonly BossConfig _bossConfig;

        // Dependencies are now explicit
        public AIBehaviorFactory(BossConfig bossConfig)
        {
            _bossConfig = bossConfig;
            RegisterBehaviorTrees();
        }

        private void RegisterBehaviorTrees()
        {
            // Example of registering a behavior tree
            _behaviorTrees["NightmareCorruption_Phase1"] = CreatePhase1Tree;
        }

        public Node CreateBehaviorTree(string name)
        {
            if (_behaviorTrees.TryGetValue(name, out var factoryMethod))
            {
                return factoryMethod();
            }
            throw new ArgumentException($"Behavior tree '{name}' not found.");
        }

        private Node CreatePhase1Tree()
        {
            // The actual logic of phase 1 is now fully encapsulated within this behavior tree.
            // We can access the config directly since it's a dependency.
            return new SequenceNode(
                // ... Phase 1 logic using _bossConfig ...
                new ActionNode(bb => NodeState.Running) 
            );
        }
    }
}