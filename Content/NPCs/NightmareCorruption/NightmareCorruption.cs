using AshenVoid.Content.Items.Drops;
using AshenVoid.Content.NPCs.NightmareCorruption.Configs;
using AshenVoid.Content.NPCs.NightmareCorruption.States;
using AshenVoid.Core.Builders;
using AshenVoid.Core.Configuration;
using AshenVoid.Core.ECS;
using AshenVoid.Core.ECS.FSM;
using AshenVoid.Core.ECS.Systems;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace AshenVoid.Content.NPCs.NightmareCorruption
{
    [AutoloadBossHead]
    public class NightmareCorruption : EcsBoss
    {
        private static BossConfig _bossConfig;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 4;

            var configLoader = new ConfigLoader();
            string configPath = $"Content/NPCs/NightmareCorruption/Configs/{nameof(NightmareCorruption)}.hjson";
            _bossConfig = configLoader.Load<BossConfig>(configPath);
        }

        public override void SetBossDefaults()
        {
            NPC.width = 242;
            NPC.height = 192;
            NPC.lifeMax = _bossConfig?.LifeMax ?? 13100;
            NPC.damage = _bossConfig?.Damage ?? 50;
            NPC.defense = _bossConfig?.Defense ?? 20;
            NPC.knockBackResist = 0f;
            NPC.value = Item.buyPrice(0, 3, 0, 0);
            NPC.boss = true;
            NPC.noGravity = true;
            NPC.noTileCollide = true;

            NPC.HitSound = new SoundStyle("AshenVoid/Assets/Sounds/Custom/NightmareCorruptionHurt");
            NPC.DeathSound = new SoundStyle("AshenVoid/Assets/Sounds/Custom/NightmareCorruptionDead");

            Music = MusicLoader.GetMusicSlot(Mod, "Assets/Music/FoulAbyssEcho");
        }

        protected override ComponentController InitializeController()
        {
            var stateFactory = new StateFactory();
            var builder = new BossBuilder();

            var aiStateComponent = new AIStateComponent(NPC, stateFactory);

            aiStateComponent.RegisterState<SpawnState>();
            aiStateComponent.RegisterState<Phase1State>();
            aiStateComponent.RegisterState<Phase2State>();
            aiStateComponent.RegisterState<DeathState>();

            aiStateComponent.Blackboard.Set("BossConfig", _bossConfig);

            builder.AddComponent(() => new MovementComponent(NPC, _bossConfig.Phase1.Movement));
            builder.AddComponent(() => new AttackComponent());
            builder.AddComponent(() => new AnimationComponent(NPC));
            builder.AddComponent(() => new VFXComponent());
            builder.AddComponent(() => new StatSheetComponent(NPC, _bossConfig));
            builder.AddComponent(() => aiStateComponent);
            builder.AddComponent(() => new HealthComponent(NPC.lifeMax));

            // Server-side systems
            builder.AddSystem(new MovementSystem());
            builder.AddSystem(new AttackSystem());
            builder.AddSystem(new AIStateSystem());
            builder.AddSystem(new StatSystem());
            builder.AddSystem(new HealthSystem());

            // Client-side systems
            builder.AddSystem(new ClientInterpolationSystem());

            // Systems that run on both
            builder.AddSystem(new AnimationSystem());

            return builder.Build();
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