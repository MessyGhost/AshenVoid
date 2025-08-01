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
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 4;
        }

        // NEW: This replaces the old SetDefaults
        public override void SetBossDefaults()
        {
            NPC.width = 242;
            NPC.height = 192;
            NPC.lifeMax = 13100;
            NPC.knockBackResist = 0f;
            NPC.value = Item.buyPrice(0, 3, 0, 0);
            NPC.boss = true;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            // NPC.netAlways and aiStyle are now set in the base class

            NPC.HitSound = new SoundStyle("AshenVoid/Assets/Sounds/Custom/NightmareCorruptionHurt");
            NPC.DeathSound = new SoundStyle("AshenVoid/Assets/Sounds/Custom/NightmareCorruptionDead");

            Music = MusicLoader.GetMusicSlot(Mod, "Assets/Music/FoulAbyssEcho");
        }

        // This is now called from SetDefaults in the base class
        protected override ComponentController InitializeController()
        {
            var configLoader = new ConfigLoader();
            var bossConfig = configLoader.LoadForBoss<BossConfig>(FullName);

            // We can now set these here, as this runs before the NPC is "live"
            NPC.damage = bossConfig.Damage;
            NPC.defense = bossConfig.Defense;

            var stateFactory = new StateFactory();
            var builder = new BossBuilder();

            var aiStateComponent = new AIStateComponent(NPC, stateFactory);

            // NEW: Register all possible states for network IDs
            aiStateComponent.RegisterState<SpawnState>();    // ID: 0
            aiStateComponent.RegisterState<Phase1State>();   // ID: 1
            aiStateComponent.RegisterState<Phase2State>();   // ID: 2
            aiStateComponent.RegisterState<DeathState>();    // ID: 3

            aiStateComponent.Blackboard.Set("BossConfig", bossConfig);
            // We no longer set initial state here. It's done in OnSpawn.

            builder.AddComponent(() => new MovementComponent(NPC, bossConfig.Phase1.Movement));
            builder.AddComponent(() => new AttackComponent());
            builder.AddComponent(() => new AnimationComponent(NPC));
            builder.AddComponent(() => new VFXComponent());
            builder.AddComponent(() => new StatSheetComponent(NPC, bossConfig));
            builder.AddComponent(() => aiStateComponent);

            // HealthComponent should be initialized with NPC.lifeMax, not NPC.life
            builder.AddComponent(() => new HealthComponent(NPC.lifeMax));

            // Systems are defined once and for all
            builder.AddSystem(new MovementSystem());
            builder.AddSystem(new AttackSystem());
            builder.AddSystem(new AIStateSystem());
            builder.AddSystem(new AnimationSystem());
            builder.AddSystem(new StatSystem());
            builder.AddSystem(new HealthSystem());

            return builder.Build();
        }

        public override void OnKill()
        {
            // This check is correct
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                Item.NewItem(NPC.GetSource_Loot(), NPC.getRect(), ModContent.ItemType<NightmareEssence>(), 10);
            }
        }
    }
}