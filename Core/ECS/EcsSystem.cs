using AshenVoid.Core.ECS.Systems;
using AshenVoid.Core.Events;
using AshenVoid.Core.Networking;
using AshenVoid.Core.ECS.FSM;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System.Collections.Generic;
using System.Linq;
using System;
using AshenVoid.Core.Builders;
using System.Reflection;

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
            RegisterAllBossStates();
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

        private void RegisterAllBossStates()
        {
            foreach (var modNpc in Mod.GetContent<ModNPC>())
            {
                if (modNpc is EcsBoss boss)
                {
                    boss.RegisterStates(StateFactory);
                }
            }
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