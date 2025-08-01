using AshenVoid.Core.ECS.Systems;
using AshenVoid.Core.Events;
using AshenVoid.Core.Networking;
using AshenVoid.Core.ECS.FSM;
using AshenVoid.Content.NPCs.NightmareCorruption.States;
using AshenVoid.Content.NPCs.NightmareCorruption.Configs;
using AshenVoid.Core.Configuration;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System.Collections.Generic;

namespace AshenVoid.Core.ECS
{
    public class EcsSystem : ModSystem
    {
        public static EcsSystem Instance { get; private set; }

        public EcsWorld World { get; private set; }
        public EventBus EventBus { get; private set; }
        public SystemManager SystemManager { get; private set; }
        public NetworkManager NetworkManager { get; private set; }
        public StateFactory StateFactory { get; private set; }

        // Global mapping from Terraria NPC ID to ECS Entity ID
        public static readonly Dictionary<int, int> NpcWhoAmIToEntityId = new();

        public override void Load()
        {
            Instance = this;
            EventBus = new EventBus();
            SystemManager = new SystemManager();
            NetworkManager = new NetworkManager(EventBus);
            StateFactory = new StateFactory();
            World = new EcsWorld(SystemManager, EventBus);

            RegisterGlobalSystems();
            RegisterNetworkEvents();
            RegisterStates();
        }

        private void RegisterGlobalSystems()
        {
            SystemManager.RegisterSystem(new MovementSystem());
            SystemManager.RegisterSystem(new AttackSystem());
            SystemManager.RegisterSystem(new AIStateSystem());
            SystemManager.RegisterSystem(new StatSystem());
            SystemManager.RegisterSystem(new HealthSystem());
            SystemManager.RegisterSystem(new ClientInterpolationSystem());
            SystemManager.RegisterSystem(new AnimationSystem());
            SystemManager.RegisterSystem(new BehaviorTreeSystem());
            SystemManager.RegisterSystem(new ClientStateSystem());
        }

        private void RegisterNetworkEvents()
        {
            NetworkManager.RegisterEventType<EntityIdSyncEvent>();
            NetworkManager.RegisterEventType<StateChangedNetworkEvent>();
            NetworkManager.RegisterEventType<AttackPerformedNetworkEvent>();
        }

        private void RegisterStates()
        {
            // States are now stateless and don't need config at registration time.
            StateFactory.RegisterState(() => new SpawnState());
            StateFactory.RegisterState(() => new Phase1State());
            StateFactory.RegisterState(() => new Phase2State());
            StateFactory.RegisterState(() => new DeathState());
        }

        public override void Unload()
        {
            World = null;
            SystemManager = null;
            EventBus = null;
            NetworkManager = null;
            StateFactory = null;
            Instance = null;
            NpcWhoAmIToEntityId.Clear();
        }

        public override void PostUpdateEverything()
        {
            World?.Update(Main.gameTimeCache);

            // Dispatch all queued events after systems have updated
            EventBus?.DispatchEvents();

            if (Main.netMode == NetmodeID.Server)
            {
                NetworkManager?.SendPendingMessages();
            }
        }
    }
}