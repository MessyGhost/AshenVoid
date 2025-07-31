using AshenVoid.Core.ECS.AI;
using AshenVoid.Core.ECS.FSM;
using AshenVoid.Core.ECS.Interfaces;
using AshenVoid.Core.Events;
using AshenVoid.Core.Stats;
using System;
using Terraria;

namespace AshenVoid.Core.ECS
{
    public class AIStateComponent : IComponent
    {
        public readonly NPC NPC;
        public ComponentController Controller { get; }
        public EventBus EventBus { get; }
        public Blackboard Blackboard { get; }
        public StateMachine StateMachine { get; }
        private readonly StateFactory _stateFactory;

        private bool _isEnraged;
        private const string RAGE_SOURCE = "Rage";

        public AIStateComponent(NPC npc, ComponentController controller, EventBus eventBus, StateFactory stateFactory)
        {
            NPC = npc;
            Controller = controller;
            EventBus = eventBus;
            StateMachine = new StateMachine();
            Blackboard = new Blackboard();
            _stateFactory = stateFactory;
        }

        public void OnDamaged(NPCDamagedEvent e)
        {
            float damageTaken = Blackboard.Get<float>("DamageTakenSinceLastDash");
            Blackboard.Set("DamageTakenSinceLastDash", damageTaken + e.Hit.Damage);
        }

        public void OnHealthLoss(NPCHealthLossEvent e)
        {
            float lastSummonHealth = Blackboard.Get<float>("LastSummonHealthPercent");
            if (lastSummonHealth == 0f) lastSummonHealth = 1f;

            if (lastSummonHealth - e.HealthPercentage >= 0.1f)
            {
                Blackboard.Set("ShouldSummon", true);
                Blackboard.Set("LastSummonHealthPercent", e.HealthPercentage);
            }

            if (e.HealthPercentage < 0.5f && !_isEnraged)
            {
                _isEnraged = true;
                var statSheet = Controller.GetComponent<IStatSheetComponent>();
                if (statSheet != null)
                {
                    var damageMod = new StatModifier(0.2f, StatModType.PercentMult, (int)StatModType.PercentMult, RAGE_SOURCE);
                    var cooldownMod = new StatModifier(-0.2f, StatModType.PercentMult, (int)StatModType.PercentMult, RAGE_SOURCE);
                    statSheet.Damage.AddModifier(damageMod);
                    statSheet.AttackCooldownMultiplier.AddModifier(cooldownMod);
                    Main.NewText($"{NPC.FullName} has become enraged! Damage and attack speed increased.");
                }
            }
        }

        public void SetInitialState(Type stateType)
        {
            Blackboard.Set(BlackboardKeys.NPC, NPC);
            Blackboard.Set(BlackboardKeys.Controller, Controller);
            Blackboard.Set(BlackboardKeys.AIState, this);

            var state = _stateFactory.GetState<IState>(stateType);
            StateMachine.ChangeState(state, Blackboard);
        }

        public void ChangeState<T>() where T : IState
        {
            var newState = _stateFactory.GetState<T>();
            StateMachine.ChangeState(newState, Blackboard);
        }
    }

    public static class StateFactoryExtensions
    {
        public static IState GetState<T>(this StateFactory factory, Type stateType) where T : IState
        {
            // This is a bit of a hack to call a generic method with a Type variable.
            // A better solution might involve a non-generic GetState method in the factory.
            var method = typeof(StateFactory).GetMethod(nameof(StateFactory.GetState), Type.EmptyTypes);
            var genericMethod = method.MakeGenericMethod(stateType);
            return (IState)genericMethod.Invoke(factory, null);
        }
    }
}