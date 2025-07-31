using AshenVoid.Content.Items.Drops;
using AshenVoid.Content.NPCs.NightmareCorruption.Configs;
using AshenVoid.Content.NPCs.NightmareCorruption.States;
using AshenVoid.Core.Builders;
using AshenVoid.Core.Configuration;
using AshenVoid.Core.ECS;
using AshenVoid.Core.ECS.FSM;
using AshenVoid.Core.ECS.Systems;
using AshenVoid.Core.Events;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace AshenVoid.Content.NPCs.NightmareCorruption
{
    [AutoloadBossHead]
    public class NightmareCorruption : ModNPC, IComponentProvider
    {
        public ComponentController ComponentController { get; private set; }
        private EventBus _eventBus;
        private int _lastHealth;

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

        public override void OnSpawn(IEntitySource source)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                InitializeController();
            }
        }

        private void InitializeController()
        {
            var configLoader = new ConfigLoader();
            var bossConfig = configLoader.LoadForBoss<BossConfig>(FullName);

            NPC.damage = bossConfig.Damage;
            NPC.defense = bossConfig.Defense;

            var eventBus = new EventBus();

            var states = new List<IState>
            {
                new SpawnState(bossConfig),
                new Phase1State(bossConfig.Phase1),
                new Phase2State(bossConfig.Phase1), // Assuming Phase2 uses Phase1 config for now
                new DeathState()
            };
            var stateFactory = new StateFactory(states);

            ComponentController = new BossBuilder()
                .AddComponent(() => new MovementComponent(NPC, bossConfig.Phase1.Movement))
                .AddComponent(() => new AttackComponent())
                .AddComponent(() => new AnimationComponent(NPC))
                .AddComponent(() => new VFXComponent())
                .AddComponent(() => new StatSheetComponent(NPC, bossConfig))
                .AddComponent(() =>
                {
                    // AIStateComponent now requires the state factory
                    var aiState = new AIStateComponent(NPC, ComponentController, eventBus, stateFactory);
                    eventBus.Subscribe<NPCDamagedEvent>(aiState.OnDamaged);
                    eventBus.Subscribe<NPCHealthLossEvent>(aiState.OnHealthLoss);
                    return aiState;
                })
                .AddSystem(new MovementSystem())
                .AddSystem(new AttackSystem())
                .AddSystem(new AIStateSystem())
                .AddSystem(new AnimationSystem())
                .AddSystem(new StatSystem())
                .WithInitialState(typeof(SpawnState))
                .Build();

            _eventBus = eventBus;
            _lastHealth = NPC.life;
        }

        public override void OnHitByItem(Player player, Item item, NPC.HitInfo hit, int damageDone)
        {
            _eventBus?.Publish(new NPCDamagedEvent(NPC, hit));
        }

        public override void OnHitByProjectile(Projectile projectile, NPC.HitInfo hit, int damageDone)
        {
            _eventBus?.Publish(new NPCDamagedEvent(NPC, hit));
        }

        public override void AI()
        {
            if (ComponentController == null) return;
            if (Main.netMode == NetmodeID.MultiplayerClient) return;

            if (NPC.life != _lastHealth)
            {
                float lastHealthPercent = (float)_lastHealth / NPC.lifeMax;
                float currentHealthPercent = (float)NPC.life / NPC.lifeMax;
                _eventBus?.Publish(new NPCHealthLossEvent(NPC, currentHealthPercent, lastHealthPercent));
                _lastHealth = NPC.life;
            }

            ComponentController.Update(Main.gameTimeCache, NPC);
        }

        public override void FindFrame(int frameHeight) { }

        public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            var vfx = ComponentController?.GetComponent<VFXComponent>();
            vfx?.PostDraw(spriteBatch);
            base.PostDraw(spriteBatch, screenPos, drawColor);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            var vfx = ComponentController?.GetComponent<VFXComponent>();
            vfx?.PreDraw(spriteBatch);
            return base.PreDraw(spriteBatch, screenPos, drawColor);
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