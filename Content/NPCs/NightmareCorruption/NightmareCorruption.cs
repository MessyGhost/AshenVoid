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

            var builder = new BossBuilder();

            // Setup AI Component first, as it holds the blackboard
            var aiStateComponent = new AIStateComponent(NPC, stateFactory);
            aiStateComponent.Blackboard.Set("BossConfig", bossConfig);
            aiStateComponent.SetInitialState(typeof(SpawnState));

            builder.AddComponent(() => new MovementComponent(NPC, bossConfig.Phase1.Movement));
            builder.AddComponent(() => new AttackComponent());
            builder.AddComponent(() => new AnimationComponent(NPC));
            builder.AddComponent(() => new VFXComponent());
            builder.AddComponent(() => new StatSheetComponent(NPC, bossConfig));
            builder.AddComponent(() => aiStateComponent); // Add the pre-configured component
            builder.AddComponent(() => new HealthComponent(NPC.life));

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
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                Item.NewItem(NPC.GetSource_Loot(), NPC.getRect(), ModContent.ItemType<NightmareEssence>(), 10);
            }
        }
    }
}