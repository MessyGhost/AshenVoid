using AshenVoid.Content.Items.Drops;
using AshenVoid.Content.NPCs.NightmareCorruption.Configs;
using AshenVoid.Content.NPCs.NightmareCorruption.States;
using AshenVoid.Core;
using AshenVoid.Core.Builders;
using AshenVoid.Core.Configuration;
using AshenVoid.Core.ECS;
using AshenVoid.Core.ECS.BehaviorTree;
using AshenVoid.Core.ECS.FSM;
using AshenVoid.Core.ECS.Systems;
using AshenVoid.Core.Events;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace AshenVoid.Content.NPCs.NightmareCorruption
{
    [AutoloadBossHead]
    public class NightmareCorruption : EcsBoss
    {
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 4;
        }

        protected override void RegisterServices(ServiceLocator services)
        {
            var configLoader = new ConfigLoader();
            string configPath = $"Content/NPCs/NightmareCorruption/Configs/{nameof(NightmareCorruption)}.hjson";
            var bossConfig = configLoader.Load<BossConfig>(configPath);
            services.Register(bossConfig);

            services.Register(new StateFactory(services));
            services.Register(new AIBehaviorFactory(services));
        }

        public override void SetBossDefaults()
        {
            var config = Services.Get<BossConfig>();
            NPC.width = 242;
            NPC.height = 192;
            NPC.lifeMax = config?.LifeMax ?? 13100;
            NPC.damage = config?.Damage ?? 50;
            NPC.defense = config?.Defense ?? 20;
            NPC.knockBackResist = 0f;
            NPC.value = Item.buyPrice(0, 3, 0, 0);
            NPC.boss = true;
            NPC.noGravity = true;
            NPC.noTileCollide = true;

            NPC.HitSound = new SoundStyle("AshenVoid/Assets/Sounds/Custom/NightmareCorruptionHurt");
            NPC.DeathSound = new SoundStyle("AshenVoid/Assets/Sounds/Custom/NightmareCorruptionDead");

            Music = MusicLoader.GetMusicSlot(Mod, "Assets/Music/FoulAbyssEcho");
        }

        protected override ComponentController InitializeController(ServiceLocator services)
        {
            var controller = new ComponentController();
            var config = services.Get<BossConfig>();

            var aiStateComponent = new AIStateComponent(NPC);
            aiStateComponent.RegisterState<SpawnState>();
            aiStateComponent.RegisterState<Phase1State>();
            aiStateComponent.RegisterState<Phase2State>();
            aiStateComponent.RegisterState<DeathState>();
            aiStateComponent.SetInitialState(typeof(SpawnState));

            controller.AddComponent(new MovementComponent(NPC, config.Phase1.Movement));
            controller.AddComponent(new AttackComponent());
            controller.AddComponent(new AnimationComponent(NPC));
            controller.AddComponent(new VFXComponent());
            controller.AddComponent(new StatSheetComponent(NPC, config));
            controller.AddComponent(aiStateComponent);
            controller.AddComponent(new HealthComponent(NPC.lifeMax));

            controller.AddSystem(new MovementSystem());
            controller.AddSystem(new AttackSystem());
            controller.AddSystem(new AIStateSystem());
            controller.AddSystem(new StatSystem());
            controller.AddSystem(new HealthSystem());
            controller.AddSystem(new ClientInterpolationSystem());
            controller.AddSystem(new AnimationSystem());
            controller.AddSystem(new NetworkEventSystem(EventBus)); // Add the new system

            return controller;
        }

        public override void OnKill()
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                Item.NewItem(NPC.GetSource_Loot(), NPC.getRect(), ModContent.ItemType<NightmareEssence>(), 10);
            }
        }
    }
}