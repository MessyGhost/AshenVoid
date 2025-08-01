using AshenVoid.Core.ECS;
using AshenVoid.Core.ECS.AI;
using AshenVoid.Core.Events;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace AshenVoid.Core.Builders
{
    public abstract class EcsBoss : ModNPC
    {
        public ComponentController Controller { get; private set; }
        public ServiceLocator Services { get; private set; }
        protected EventBus EventBus { get; private set; }

        // This method is where you define all the components and systems for this boss.
        protected abstract ComponentController InitializeController(ServiceLocator services);

        // This method is for registering all necessary services for the boss.
        protected abstract void RegisterServices(ServiceLocator services);

        public sealed override void SetDefaults()
        {
            EventBus = new EventBus();
            Services = new ServiceLocator();

            // Register common services first
            Services.Register(EventBus);

            // Register boss-specific services
            RegisterServices(Services);

            // Initialize the controller, passing in the services
            Controller = InitializeController(Services);

            // Set ModNPC specific defaults
            SetBossDefaults();

            NPC.aiStyle = -1;
            NPC.netAlways = true;

            // The system cache should be built once all components and systems are registered.
            Controller.BuildSystemCache();
            
            // Put the service locator in the blackboard for easy access from states/systems
            var aiState = Controller.GetComponent<AIStateComponent>();
            if (aiState != null)
            {
                aiState.Blackboard.Set(BlackboardKeys.ServiceLocator, Services);
            }
        }

        public abstract void SetBossDefaults();

        public override void OnSpawn(IEntitySource source)
        {
            // OnSpawn logic, if any, should be handled by a dedicated System.
        }

        public override void AI()
        {
            Controller?.Update(Main.gameTimeCache, NPC, EventBus);
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            var networkedComponents = Controller?.GetNetworkedComponents();
            if (networkedComponents != null)
            {
                foreach (var component in networkedComponents)
                {
                    component.SendData(NPC, writer);
                }
            }
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            var networkedComponents = Controller?.GetNetworkedComponents();
            if (networkedComponents != null)
            {
                foreach (var component in networkedComponents)
                {
                    component.ReceiveData(NPC, reader);
                }
            }
        }

        public override void OnHitByItem(Player player, Item item, NPC.HitInfo hit, int damageDone)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
                EventBus?.Publish(new NPCDamagedEvent(NPC, hit));
        }

        public override void OnHitByProjectile(Projectile projectile, NPC.HitInfo hit, int damageDone)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
                EventBus?.Publish(new NPCDamagedEvent(NPC, hit));
        }
    }
}