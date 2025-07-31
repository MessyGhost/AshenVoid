using AshenVoid.Core.ECS;
using AshenVoid.Content.Items.Drops;
using AshenVoid.Content.NPCs.NightmareCorruption.States;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using AshenVoid.Content.NPCs.NightmareCorruption.Configs;
using AshenVoid.Core.Configuration;
using AshenVoid.Core.DI;
using AshenVoid.Core.ECS.Interfaces;
using AshenVoid.Core.Events;

namespace AshenVoid.Content.NPCs.NightmareCorruption
{
    [AutoloadBossHead]
    public partial class NightmareCorruption : ModNPC
    {
        protected ComponentController Components { get; private set; }
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
            if (Components == null)
            {
                SetupECS();
            }

            if (Main.netMode == NetmodeID.MultiplayerClient) return;

            // Publish health loss event if health has changed
            if (NPC.life != _lastHealth)
            {
                float lastHealthPercent = (float)_lastHealth / NPC.lifeMax;
                float currentHealthPercent = (float)NPC.life / NPC.lifeMax;
                _eventBus?.Publish(new NPCHealthLossEvent(NPC, currentHealthPercent, lastHealthPercent));
                _lastHealth = NPC.life;
            }

            Components.Update();
        }

        private void SetupECS()
        {
            var builder = new Core.Builders.BossBuilder(this);

            Components = builder
                .WithServices(services =>
                {
                    // 注册通用的、可重用的服务
                    services.RegisterSingleton<IConfigLoader, ConfigLoader>();
                    services.RegisterSingleton<EventBus, EventBus>();

                    // 加载并注册此Boss特有的配置
                    var configLoader = services.GetService<IConfigLoader>();
                    var bossConfig = configLoader.Load<BossConfig>("Content/NPCs/NightmareCorruption/Configs/NightmareCorruption.hjson");
                    services.RegisterInstance(bossConfig);
                    services.RegisterInstance(bossConfig.Phase1.Movement);
                    services.RegisterInstance(bossConfig.Phase1.Attacks);

                    // 根据配置设置NPC属性
                    NPC.damage = bossConfig.Damage;
                    NPC.defense = bossConfig.Defense;

                    // 注册此Boss所需的组件
                    services.RegisterSingleton<IMovementComponent, MovementComponent>();
                    services.RegisterSingleton<IAttackComponent, AttackComponent>();
                    services.RegisterSingleton<IAnimationComponent, AnimationComponent>();
                    services.RegisterSingleton<IVFXComponent, VFXComponent>();
                    services.RegisterSingleton<IStatSheetComponent, StatSheetComponent>();
                    services.RegisterSingleton<AIComponent, AIComponent>();
                })
                .WithInitialState(new SpawnState(builder.GetService<BossConfig>())) // 从构建器中获取依赖
                .Build();

            // 从完全配置好的组件中获取服务
            _eventBus = Components.GetComponent<AIComponent>().EventBus;
            _lastHealth = NPC.life;
        }

        public override void FindFrame(int frameHeight)
        {
            // Animation is now handled by the AnimationComponent
        }

        public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            var vfx = Components.GetComponent<VFXComponent>();
            vfx?.PostDraw(spriteBatch);
            base.PostDraw(spriteBatch, screenPos, drawColor);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            var vfx = Components.GetComponent<VFXComponent>();
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