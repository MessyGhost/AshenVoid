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
        private BossConfig _config;
        private ServiceContainer _serviceContainer;
        private EventBus _eventBus;
        private int _lastHealth;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 4;
        }

        public override void SetDefaults()
        {
            _config = ConfigLoader.Load<BossConfig>("NightmareCorruption.hjson");

            _serviceContainer = new ServiceContainer();
            RegisterServices();

            NPC.width = 242;
            NPC.height = 192;
            NPC.lifeMax = 13100;
            NPC.damage = _config.Damage;
            NPC.defense = _config.Defense;
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

        private void RegisterServices()
        {
            _serviceContainer.RegisterSingleton<EventBus, EventBus>();
            _serviceContainer.RegisterSingleton<IMovementComponent, MovementComponent>();
            _serviceContainer.RegisterSingleton<IAttackComponent, AttackComponent>();
            _serviceContainer.RegisterSingleton<IAnimationComponent, AnimationComponent>();
            _serviceContainer.RegisterSingleton<IVFXComponent, VFXComponent>();
            _serviceContainer.RegisterSingleton<IStatSheetComponent, StatSheetComponent>();
            _serviceContainer.RegisterSingleton<AIComponent, AIComponent>();

            _serviceContainer.RegisterInstance(_config);
            _serviceContainer.RegisterInstance(NPC);
            _serviceContainer.RegisterInstance(_config.Phase1.Movement);
            _serviceContainer.RegisterInstance(_config.Phase1.Attacks);
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
            // 1. Create a ComponentController instance
            Components = new ComponentController(NPC, _serviceContainer);

            // 2. Let the controller create all components
            Components.Initialize();

            // 3. Get services from the container
            var ai = Components.GetComponent<AIComponent>();
            _eventBus = _serviceContainer.GetService<EventBus>();

            // 4. Set the initial state for the AI
            ai.SetInitialState(new Phase1State(_config.Phase1));

            // 5. Initialize last health
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