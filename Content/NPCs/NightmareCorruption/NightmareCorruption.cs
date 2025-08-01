using AshenVoid.Content.Items.Drops;
using AshenVoid.Content.NPCs.NightmareCorruption.Configs;
using AshenVoid.Content.NPCs.NightmareCorruption.States;
using AshenVoid.Core.Builders;
using AshenVoid.Core.Configuration;
using AshenVoid.Core.ECS;
using AshenVoid.Core.ECS.AI;
using AshenVoid.Core.ECS.FSM;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace AshenVoid.Content.NPCs.NightmareCorruption
{
    [AutoloadBossHead]
    public class NightmareCorruption : EcsBoss
    {
        private BossConfig _config;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 6;

            NPCID.Sets.MPAllowedEnemies[Type] = true;
            NPCID.Sets.BossBestiaryPriority.Add(Type);

            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
        }

        public override void SetBossDefaults()
        {
            _config = ConfigLoader.Instance.Load<BossConfig>($"Content/NPCs/NightmareCorruption/Configs/NightmareCorruption.hjson");

            NPC.width = 110;
            NPC.height = 110;
            NPC.damage = _config.Damage;
            NPC.defense = _config.Defense;
            NPC.lifeMax = _config.LifeMax;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.knockBackResist = 0f;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.value = Item.buyPrice(0, 15, 0, 0);
            NPC.boss = true;
            NPC.npcSlots = 10f;

            if (!Main.dedServ)
            {
                Music = MusicLoader.GetMusicSlot(Mod, "Assets/Music/FoulAbyssEcho");
            }
        }

        protected override void BuildEntity(EcsWorld world, int entityId)
        {
            world.AddComponent(entityId, new StatSheetComponent(NPC, _config));
            world.AddComponent(entityId, new HealthComponent(NPC.lifeMax, NPC.lifeMax));
            world.AddComponent(entityId, new MovementComponent(NPC, _config.Phase1.Movement));
            world.AddComponent(entityId, new AnimationComponent(NPC));
            world.AddComponent(entityId, new AttackComponent(1));
            world.AddComponent(entityId, new AIStateComponent(typeof(SpawnState), EcsSystem.Instance.StateFactory));
            world.AddComponent(entityId, new AIBlackboardComponent());
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.BossBag(ModContent.ItemType<NightmareEssence>()));
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<NightmareEssence>(), 1, 5, 10));
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Times.NightTime,
                new FlavorTextBestiaryInfoElement("A swirling mass of nightmares, given form and purpose.")
            });
        }
    }
}