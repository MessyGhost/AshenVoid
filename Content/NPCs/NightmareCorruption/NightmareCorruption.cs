using AshenVoid.Content.Items.Drops;
using AshenVoid.Content.NPCs.NightmareCorruption.Configs;
using AshenVoid.Content.NPCs.NightmareCorruption.States;
using AshenVoid.Core.Builders;
using AshenVoid.Core.Configuration;
using AshenVoid.Core.ECS;
using AshenVoid.Core.ECS.AI;
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

        public override void SetBossDefaults()
        {
            var bossConfig = ConfigLoader.Instance.Load<BossConfig>($"Content/NPCs/NightmareCorruption/Configs/{nameof(NightmareCorruption)}.hjson");

            NPC.width = 242;
            NPC.height = 192;
            NPC.lifeMax = bossConfig?.LifeMax ?? 13100;
            NPC.damage = bossConfig?.Damage ?? 50;
            NPC.defense = bossConfig?.Defense ?? 20;
            NPC.knockBackResist = 0f;
            NPC.value = Item.buyPrice(0, 3, 0, 0);
            NPC.boss = true;
            NPC.noGravity = true;
            NPC.noTileCollide = true;

            NPC.HitSound = new SoundStyle("AshenVoid/Assets/Sounds/Custom/NightmareCorruptionHurt");
            NPC.DeathSound = new SoundStyle("AshenVoid/Assets/Sounds/Custom/NightmareCorruptionDead");

            Music = MusicLoader.GetMusicSlot(Mod, "Assets/Music/FoulAbyssEcho");
        }

        protected override void BuildEntity(EcsWorld world, int entityId)
        {
            var bossConfig = ConfigLoader.Instance.Load<BossConfig>($"Content/NPCs/NightmareCorruption/Configs/{nameof(NightmareCorruption)}.hjson");
            var stateFactory = EcsSystem.Instance.StateFactory;

            var aiStateComponent = new AIStateComponent(NPC, stateFactory);
            aiStateComponent.SetInitialState(typeof(SpawnState));

            world.AddComponent(entityId, new MovementComponent(NPC, bossConfig.Phase1.Movement));
            world.AddComponent(entityId, new AttackComponent());
            world.AddComponent(entityId, new AnimationComponent(NPC));
            world.AddComponent(entityId, new VFXComponent());
            world.AddComponent(entityId, new StatSheetComponent(NPC, bossConfig));
            world.AddComponent(entityId, aiStateComponent);
            world.AddComponent(entityId, new HealthComponent(NPC.lifeMax));
            world.AddComponent(entityId, new AIBlackboardComponent());
        }

        public override void OnKill()
        {
            base.OnKill(); // Call the base method to destroy the entity
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                Item.NewItem(NPC.GetSource_Loot(), NPC.getRect(), ModContent.ItemType<NightmareEssence>(), 10);
            }
        }
    }
}