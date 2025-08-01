using AshenVoid.Core.ECS.Systems;
using AshenVoid.Core.Events;
using AshenVoid.Core.Networking;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace AshenVoid.Core.ECS
{
    public class EcsSystem : ModSystem
    {
        public static EcsSystem Instance { get; private set; }

        public EcsWorld World { get; private set; }
        public EventBus EventBus { get; private set; }
        public SystemManager SystemManager { get; private set; }
        public NetworkManager NetworkManager { get; private set; }

        public override void Load()
        {
            Instance = this;
            EventBus = new EventBus();
            SystemManager = new SystemManager();
            NetworkManager = new NetworkManager(EventBus);
            World = new EcsWorld(SystemManager, EventBus);

            RegisterGlobalSystems();
            RegisterNetworkEvents();
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
        }

        private void RegisterNetworkEvents()
        {
            // Register all network-synchronized events here.
            // This ensures a consistent ID between server and client.
            // Example:
            // NetworkManager.RegisterEventType<MyNetworkEvent>(0);
        }

        public override void Unload()
        {
            World = null;
            SystemManager = null;
            EventBus = null;
            NetworkManager = null;
            Instance = null;
        }

        public override void PostUpdateEverything()
        {
            World?.Update(Main.gameTimeCache);

            if (Main.netMode == NetmodeID.Server)
            {
                NetworkManager?.SendPendingMessages();
            }
        }
    }
}