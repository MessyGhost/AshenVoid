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
                new ActionNode(() =>
                {
                    PlaySound(new SoundStyle("AshenVoid/Assets/Sounds/Custom/NightmareCorruptionSpawn"));
                    return NodeState.Success;
                }),
                new ActionNode(() =>
                {
                    if (NPC.alpha > 0)
                    {
                        NPC.alpha -= 5;
                        // Main.NewText($"alpha:{NPC.alpha}");
                        return NodeState.Running;
                    }
                    return NodeState.Success;
                }),
                new ActionNode(() =>
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
                // new SequenceNode(
                //     new ConditionNode(() =>
                //         Vector2.Distance(NPC.Center, TargetPlayer.Center) > 800f
                //     )
                // ),
                new ParallelNode(
                    new RepeatNode(
                        new SequenceNode(
                            DriftingBehavior(),
                            new WaitFramesNode(120)
                        ), -1),
                    new RepeatNode(
                        new SequenceNode(
                            new RepeatNode(
                                new SequenceNode(
                                    new ActionNode(ShootTowardPlayer),
                                    new WaitFramesNode(23)
                                    )
                                , 4
                            ),
                            new WaitFramesNode(79)
                            ),
                        -1
                    )
                )
            );
        }

        private Node CreatePhase2Behavior()
        {
            return new ParallelNode(
                new ActionNode(() => MoveToPosition(() => TargetPlayer.Center, 50f, 42f, new Vector2(1.0f, 0.5f), new Vector2(1.0f, 0.9f))),
                new SequenceNode(
                    new WaitFramesNode(5 * 60),
                    new ActionNode(() => SummonMinions(ModContent.NPCType<NightmarePhantom>(), 2))
                )
            );
        }

        private float offsetX;
        private Node DriftingBehavior()
        {
            return new ActionNode(() => RandomWander(TargetPlayer.Center + new Vector2(0, -200), 200f));
            return new SequenceNode(
                new OnceNode(() =>
                {
                    offsetX = (NPC.Center.X < TargetPlayer.Center.X) ? 200 : -200;
                    offsetX += Main.rand.Next(-20, 20);
                    return NodeState.Success;
                }),
                new ActionNode(() =>
                {
                    Vector2 targetPosition = new Vector2(
                        TargetPlayer.Center.X + offsetX,
                        TargetPlayer.Center.Y - 150
                    );

                    var moveState = MoveToPosition(() => targetPosition, 10f, 15f, new Vector2(1.0f, 0.5f), new Vector2(1.0f, 0.9f));
                    return moveState;
                })
            );
        }
        private NodeState ShootTowardPlayer()
        {
            if (TargetPlayer == null) return NodeState.Failure;

            Vector2 direction = (TargetPlayer.Center - NPC.Center).SafeNormalize(Vector2.UnitY);
            direction = direction.RotatedByRandom(MathHelper.ToRadians(5));

            int projType = ProjectileID.CorruptSpray;// ModContent.ProjectileType<NightmareBolt>();
            const int damage = 30; // 提升伤害

            return ShootProjectile(projType, direction, 12f, damage);
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

        public override void FindFrame(int frameHeight)
        {
            base.FindFrame(frameHeight);

            // 根据速度方向计算倾斜角度
            float tiltAngle = MathHelper.Clamp(NPC.velocity.X * 0.05f, -MathHelper.PiOver4 * 2, MathHelper.PiOver4 * 2);
            NPC.rotation = tiltAngle;
        }
    }
}