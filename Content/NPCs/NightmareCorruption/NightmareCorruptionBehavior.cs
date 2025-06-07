using System;
using AshenVoid.Content.Items.Drops;
using AshenVoid.Core.BehaviorTree;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using static AshenVoid.Core.BehaviorTree.NodeBuilder;

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
            return new SequenceNode(
                Once(() =>
                {
                    PlaySound(new SoundStyle("AshenVoid/Assets/Sounds/Custom/NightmareCorruptionSpawn"));
                }),
                Parallel(
                    FadeIn(1f),
                    Breathe(1.5f),
                    Shake(1.5f),
                    OnceApproach(() => TargetPlayer.Center + new Vector2(0, -300))
                    ),
                Once(() =>
                {
                    ChangePhase(1);
                })
            );
        }

        private int damageTaken = 0;
        private int healthThreshold = 0;
        private Node CreatePhase1Behavior()
        {
            return Fallback(
                DoUntil(
                    Chase(() => TargetPlayer.Center),
                    () => Vector2.Distance(NPC.Center, TargetPlayer.Center) > 800f
                    ),
                Parallel(
                    Interval(
                        Sequence(
                            Sequence(
                                OnceRandomApproach(() => TargetPlayer.Center + new Vector2(0, -300)),
                                DoSeconds(OrbitAround(() => TargetPlayer.Center), 3f)
                            ),
                            Random(
                                RushTowardsWithBuildUp(() => 1.5f * TargetPlayer.Center - 0.5f * NPC.Center),
                                DoSeconds(Approach(() => TargetPlayer.Center), 3f)
                            )
                        ),
                        DoSeconds(Approach(() => TargetPlayer.Center), 2f)
                        ),
                    Interval(
                        Interval(ShootTowardPlayer(), Main.rand.NextFloat(0.1f, 0.2f), Main.rand.Next(3, 6)),
                        2
                    )
                )
            );
        }

        private Node CreatePhase2Behavior()
        {
            return Chase(() => TargetPlayer.Center);
        }

        private Node ShootTowardPlayer()
        {
            return Once(() =>
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