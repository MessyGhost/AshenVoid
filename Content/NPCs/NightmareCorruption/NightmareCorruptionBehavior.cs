using System;
using AshenVoid.Content.Items.Drops;
using AshenVoid.Core.BehaviorTree;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace AshenVoid.Content.NPCs.NightmareCorruption
{
    public partial class NightmareCorruption : BossBase
    {
        protected override void CreateBehaviorTree()
        {
            phaseBehaviors.Clear();

            // 出生阶段行为树
            phaseBehaviors[0] = CreateSpawnBehavior();

            // 一阶段行为树
            phaseBehaviors[1] = CreatePhase1Behavior();

            // 二阶段行为树
            phaseBehaviors[2] = CreatePhase2Behavior();
        }

        private Node CreateSpawnBehavior()
        {
            NPC.alpha = 255;

            return new SequenceNode(
                new OnceNode(() =>
                {
                    PlaySound(new SoundStyle("AshenVoid/Assets/Sounds/Custom/NightmareCorruptionSpawn"));
                    return NodeState.Success;
                }),
                // 淡入动画
                new ActionNode(() =>
                {
                    if (NPC.alpha > 0)
                    {
                        NPC.alpha -= 5;
                        return NodeState.Running;
                    }
                    return NodeState.Success;
                }),
                new OnceNode(() =>
                {
                    ChangePhase(1);
                    return NodeState.Success;
                })
            );
        }

        private int damageTaken = 0;
        private int healthThreshold = 0;
        private Node CreatePhase1Behavior()
        {
            return new FallbackNode(
                new SequenceNode(
                    new ConditionNode(() =>
                        Vector2.Distance(NPC.Center, TargetPlayer.Center) > 800f
                    ),
                    RushTowards(TargetPlayer.Center)
                ),
                new ParallelNode(
                    new IntervalNode(RandomWander(TargetPlayer.Center + new Vector2(0, -200), 300f), 1.0f, -1),
                    new IntervalNode(
                        new IntervalNode(
                            ShootTowardPlayer(), 0.2f, 4
                        )
                        , 2.5f
                    )
                )
            );
        }

        private Node CreatePhase2Behavior()
        {
            return new ParallelNode(
                new SequenceNode(
                    new ActionNode(() => SummonMinions(ModContent.NPCType<NightmarePhantom>(), 2))
                )
            );
        }

        private Node ShootTowardPlayer()
        {
            return new ActionNode(() =>
            {
                if (TargetPlayer == null) return NodeState.Failure;

                Vector2 direction = (TargetPlayer.Center - NPC.Center).SafeNormalize(Vector2.UnitY);
                direction = direction.RotatedByRandom(MathHelper.ToRadians(5));

                int projType = ProjectileID.CorruptSpray;// ModContent.ProjectileType<NightmareBolt>();
                const int damage = 30; // 提升伤害

                return ShootProjectile(projType, direction, 12f, damage);
            });
        }

        private void TriggerGraspOfTrance()
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;

            for (int i = 0; i < 2; i++)
            {  // 双召唤增强
                Vector2 spawnPos = TargetPlayer.Center + new Vector2(
                    Main.rand.Next(-100, 100), 1000);

                int npcID = NPC.NewNPC(NPC.GetSource_FromAI(),
                    (int)spawnPos.X, (int)spawnPos.Y,
                    ModContent.NPCType<GraspOfTrance>());

                NPC graspNpc = Main.npc[npcID];
                graspNpc.ai[0] = TargetPlayer.whoAmI;
                graspNpc.ai[1] = NPC.whoAmI;
            }
        }
        public override void OnHitByItem(Player player, Item item, NPC.HitInfo hit, int damageDone)
        {
            damageTaken += damageDone;
            // 每损失10%生命值触发
            int currentThreshold = NPC.lifeMax - NPC.life;
            if (currentThreshold - healthThreshold >= NPC.lifeMax / 10)
            {
                healthThreshold = currentThreshold;
                TriggerGraspOfTrance();
            }
            base.OnHitByItem(player, item, hit, damageDone);
            SoundEngine.PlaySound(new SoundStyle("AshenVoid/Assets/Sounds/Custom/NightmareCorruptionHurt"), NPC.Center);
        }

        public override void OnHitByProjectile(Projectile projectile, NPC.HitInfo hit, int damageDone)
        {
            damageTaken += damageDone;
            // 每损失10%生命值触发
            int currentThreshold = NPC.lifeMax - NPC.life;
            if (currentThreshold - healthThreshold >= NPC.lifeMax / 10)
            {
                healthThreshold = currentThreshold;
                TriggerGraspOfTrance();
            }
            base.OnHitByProjectile(projectile, hit, damageDone);
            SoundEngine.PlaySound(new SoundStyle("AshenVoid/Assets/Sounds/Custom/NightmareCorruptionHurt"), NPC.Center);
        }

        public override void OnKill()
        {
            // 掉落物品
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                Item.NewItem(NPC.GetSource_Loot(), NPC.getRect(), ModContent.ItemType<NightmareEssence>(), 10);
            }
        }
    }
}