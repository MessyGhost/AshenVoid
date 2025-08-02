using AshenVoid.Content.Items.Drops;
using AshenVoid.Content.NPCs.NightmareCorruption.Configs;
using AshenVoid.Content.NPCs.NightmareCorruption.States;
using AshenVoid.Core.Abilities;
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

        public override void RegisterStates(StateFactory factory)
        {
            factory.RegisterState(() => new SpawnState());
            factory.RegisterState(() => new Phase1State());
            factory.RegisterState(() => new Phase2State());
            factory.RegisterState(() => new DeathState());
        }

        public override void SetBossDefaults()
        {
            _config = ConfigLoader.Instance.LoadBossConfig<BossConfig>(this);

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
            this.AiFactory = new AIBehaviorFactory(_config);
            world.AddComponent(entityId, new HealthComponent { CurrentHealth = NPC.lifeMax, MaxHealth = NPC.lifeMax });
            world.AddComponent(entityId, new MovementComponent(NPC, _config.Phase1.Movement));
            world.AddComponent(entityId, new AnimationComponent(NPC));
            world.AddComponent(entityId, new AttackComponent());
            world.AddComponent(entityId, new ChildrenComponent());
            world.AddComponent(entityId, new AIStateComponent(typeof(SpawnState), EcsSystem.Instance.StateFactory));
            world.AddComponent(entityId, new TargetComponent());
            world.AddComponent(entityId, new AIBlackboardComponent());

            var abilityComponent = new AbilityComponent();
            var abilityFactory = EcsSystem.Instance.AbilityFactory;
            foreach (var abilityConfig in _config.Abilities)
            {
                var ability = abilityFactory.CreateAbility(abilityConfig);
                abilityComponent.AddAbility(ability);
            }
            world.AddComponent(entityId, abilityComponent);
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