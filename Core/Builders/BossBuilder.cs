using AshenVoid.Content.NPCs.NightmareCorruption.Configs;
using AshenVoid.Core.Configuration;
using AshenVoid.Core.ECS;
using AshenVoid.Core.ECS.FSM;
using AshenVoid.Core.ECS.Systems;
using AshenVoid.Core.Events;
using Terraria.ModLoader;

namespace AshenVoid.Core.Builders
{
    public class BossBuilder
    {
        private readonly ModNPC _npc;
        private IState _initialState;
        private BossConfig _bossConfig;

        public BossBuilder(ModNPC npc)
        {
            _npc = npc;
        }

        public BossBuilder WithInitialState(IState state)
        {
            _initialState = state;
            return this;
        }

        public BossBuilder WithConfig(BossConfig config)
        {
            _bossConfig = config;
            return this;
        }

        public ComponentController Build()
        {
            // Manual Dependency Injection
            var eventBus = new EventBus();
            var controller = new ComponentController();

            // Create and Register Components
            var movementComponent = new MovementComponent(_npc.NPC, _bossConfig.Phase1.Movement);
            var attackComponent = new AttackComponent();
            var animationComponent = new AnimationComponent(_npc.NPC);
            var vfxComponent = new VFXComponent();
            var statSheetComponent = new StatSheetComponent(_npc.NPC, _bossConfig);
            var aiStateComponent = new AIStateComponent(_npc.NPC, controller, eventBus);

            controller.RegisterComponent(movementComponent);
            controller.RegisterComponent(attackComponent);
            controller.RegisterComponent(animationComponent);
            controller.RegisterComponent(vfxComponent);
            controller.RegisterComponent(statSheetComponent);
            controller.RegisterComponent(aiStateComponent);

            // Initialize components that need it
            eventBus.Subscribe<NPCDamagedEvent>(aiStateComponent.OnDamaged);
            eventBus.Subscribe<NPCHealthLossEvent>(aiStateComponent.OnHealthLoss);

            // Register systems
            controller.RegisterSystem(new MovementSystem());
            controller.RegisterSystem(new AttackSystem());
            controller.RegisterSystem(new AIStateSystem());
            controller.RegisterSystem(new AnimationSystem());
            controller.RegisterSystem(new StatSystem());

            // Set initial state
            if (_initialState != null)
            {
                aiStateComponent.SetInitialState(_initialState);
            }
            else
            {
                ModContent.GetInstance<AshenVoid>().Logger.Warn("No initial state provided for the boss.");
            }

            return controller;
        }
    }
}