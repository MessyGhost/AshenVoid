using AshenVoid.Content.Items.Drops;
using AshenVoid.Content.NPCs.NightmareCorruption.Configs;
using AshenVoid.Content.NPCs.NightmareCorruption.States;
using AshenVoid.Core.Builders;
using AshenVoid.Core.Configuration;
using AshenVoid.Core.ECS;
using AshenVoid.Core.ECS.AI;
using AshenVoid.Core.ECS.FSM;
using AshenVoid.Core.ECS.Systems;
using AshenVoid.Core.Events;
using Terraria;
using Terraria.ID;
using Terraria.Audio;
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

        public override void SetDefaults()
        {
            NPC.width = 242;
            NPC.height = 192;
            NPC.lifeMax = 13100;
            NPC.knockBackResist = 0f;
            NPC.value = Item.buyPrice(0, 3, 0, 0);
            NPC.boss = true;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.netAlways = true;
            NPC.aiStyle = -1;

            NPC.HitSound = new SoundStyle("AshenVoid/Assets/Sounds/Custom/NightmareCorruptionHurt");
            NPC.DeathSound = new SoundStyle("AshenVoid/Assets/Sounds/Custom/NightmareCorruptionDead");

            Music = MusicLoader.GetMusicSlot(Mod, "Assets/Music/FoulAbyssEcho");
        }

        protected override ComponentController InitializeController()
        {
            var configLoader = new ConfigLoader();
            var bossConfig = configLoader.LoadForBoss<BossConfig>(FullName);

            NPC.damage = bossConfig.Damage;
            NPC.defense = bossConfig.Defense;

            var stateFactory = new StateFactory();
            var blackboardSystem = new AIBlackboardSystem();

            var controller = new BossBuilder()
                .AddComponent(() => new MovementComponent(NPC, bossConfig.Phase1.Movement))
                .AddComponent(() => new AttackComponent())
                .AddComponent(() => new AnimationComponent(NPC))
                .AddComponent(() => new VFXComponent())
                .AddComponent(() => new StatSheetComponent(NPC, bossConfig))
                .AddComponent(() => new AIStateComponent(NPC, stateFactory))
                .AddComponent(() => new HealthComponent(NPC.life))
                .AddSystem(new MovementSystem())
                .AddSystem(new AttackSystem())
                .AddSystem(new AIStateSystem())
                .AddSystem(new AnimationSystem())
                .AddSystem(new StatSystem())
                .AddSystem(new HealthSystem())
                .AddSystem(blackboardSystem)
                .WithInitialState(typeof(SpawnState))
                .WithBlackboardData("BossConfig", bossConfig)
                .OnBuild(c => blackboardSystem.Initialize(EventBus))
                .Build();

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