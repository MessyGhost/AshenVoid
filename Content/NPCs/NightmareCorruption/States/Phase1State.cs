using AshenVoid.Core.ECS;
using AshenVoid.Core.ECS.BehaviorTree;
using AshenVoid.Core.ECS.FSM;
using Terraria;
using static AshenVoid.Core.ECS.BehaviorTree.NodeBuilder;
using AshenVoid.Content.NPCs.NightmareCorruption.Configs;

namespace AshenVoid.Content.NPCs.NightmareCorruption.States
{
    public class Phase1State : IState
    {
        private readonly PhaseConfig _config;
        private Node _behaviorTree;

        public Phase1State(PhaseConfig config)
        {
            _config = config;
        }

        public void Enter(AIComponent ai)
        {
            // Build the behavior tree for this phase
            _behaviorTree = Fallback(
                // High-priority: Chase player if too far
                Sequence(
                    Inverter(BT.IsPlayerInRange(ai, _config.Movement.ChaseDistanceFar)),
                    BT.SetChase(ai, () => ai.Target.Center, _config.Movement.ChaseStopDistance)
                ),
                // Attack logic: If ready, shoot at the player
                Sequence(
                    BT.IsAttackReady(ai), // Check cooldown
                    BT.SetShootProjectile(ai, () => ai.Target.Center, _config.Attacks.BasicShot)
                ),
                // Main combat loop
                Sequence(
                    // Alternate between orbiting and chasing
                    Weighted(
                        (node: BT.SetOrbit(ai, () => ai.Target.Center, _config.Movement.OrbitRadius), weight: 3),
                        (node: BT.SetChase(ai, () => ai.Target.Center, _config.Movement.ChaseStopDistance), weight: 2)
                    ),
                    Wait(Main.rand.NextFloat(_config.Movement.ThinkIntervalMin, _config.Movement.ThinkIntervalMax))
                )
            );
        }

        public void Update()
        {
            _behaviorTree?.Evaluate();
        }

        public void Exit()
        {
            // Cleanup when exiting phase 1
            _behaviorTree = null;
        }
    }
}