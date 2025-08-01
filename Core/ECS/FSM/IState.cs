using AshenVoid.Content.NPCs.NightmareCorruption;
using AshenVoid.Core.ECS.AI;

namespace AshenVoid.Core.ECS.FSM
{
    public interface IState
    {
        BehaviorTree.Node BehaviorTree { get; }
        void Enter(int entityId, EcsWorld world, AIBehaviorFactory factory);
        void Exit(int entityId, EcsWorld world);
        System.Type CheckTransitions(int entityId, EcsWorld world);
    }
}