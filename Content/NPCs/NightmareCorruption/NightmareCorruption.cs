using AshenVoid.Content.Items.Drops;
using AshenVoid.Content.Projectiles;
using AshenVoid.Core.BehaviorTree;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Threading;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace AshenVoid.Content.NPCs.NightmareCorruption
{
    [AutoloadBossHead]
    public class NightmareCorruption : BossBase
    {
        // 快照坐标（用于类似定点攻击）
        Vector2 snapshotPos;

        // 状态变量
        private bool _isAppearing = true;
        private float _appearingProgress = 0f;
        private float _damageAccumulator = 0f; // 用于冲刺触发的伤害累计
        private bool _isSummoningMinions = false; // 是否正在召唤恍惚之握
        private bool _isChasing = false; // 是否正在追击
        private float _chaseSpeedMultiplier = 1f; // 追击速度倍率
        private int _dashCooldown = 0; // 冲刺冷却
        private int _summonCooldown = 0; // 召唤冷却
        private int _deathTimer = 0; // 死亡动画计时器

        // 幻影管理
        private List<int> _phantoms = new List<int>(); // 存储幻影NPC的whoAmI

        // 冲刺状态
        private bool _isDashing = false;
        private int _dashDuration = 0;
        private Vector2 _dashDirection = Vector2.Zero;

        // 下砸状态
        private bool _isSlamPreparing = false;
        private bool _isSlamming = false;

        public override void SetDefaults()
        {
            NPC.width = 242;
            NPC.height = 192;
            NPC.lifeMax = 13100;
            NPC.damage = 52;
            NPC.defense = 10;
            NPC.knockBackResist = 0f;
            NPC.value = Item.buyPrice(0, 3, 0, 0);
            NPC.boss = true;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.netAlways = true;
            NPC.aiStyle = -1;
            NPC.alpha = 255;

            NPC.HitSound = new SoundStyle("AshenVoid/Assets/Sounds/Custom/NightmareCorruptionHurt");
            NPC.DeathSound = new SoundStyle("AshenVoid/Assets/Sounds/Custom/NightmareCorruptionDead");

            Main.npcFrameCount[NPC.type] = 4;

            Music = MusicLoader.GetMusicSlot(Mod, "Music/FoulAbyssEcho");
        }

        protected override void CreateBehaviorTree()
        {
            // 使用链式构建器简化语法
            phaseBehaviors[0] = BehaviorTreeBuilder.Sequence(
                // 等待登场动画完成
                BehaviorTreeBuilder.Sequence(
                    BehaviorTreeBuilder.Condition(() => _isAppearing),
                    new ActionNode(PlayAppearingAnimation),
                    BehaviorTreeBuilder.WaitUntil(() => NPC.alpha <= 0)
                ),
                // 淡入后等待15帧稳定动画
                BehaviorTreeBuilder.WaitFrames(15),
                // 进入追击模式
                new ActionNode(() => ChaseTarget(1f))
            );

            // 阶段2行为树示例
            phaseBehaviors[1] = BehaviorTreeBuilder.Sequence(
                // 基础追击
                new ActionNode(() => ChaseTarget(1f)),
                // 每30秒召唤小怪
                BehaviorTreeBuilder.Repeat(
                    BehaviorTreeBuilder.Sequence(
                        new ActionNode(() => SummonMinions(ModContent.NPCType<GraspOfTrance>(), 3)),
                        BehaviorTreeBuilder.WaitFrames(60 * 30)
                    ),
                    -1 // 无限重复
                )
            );
        }

        public override void AI()
        {
            // 确保有目标玩家
            if (NPC.target < 0 || NPC.target >= Main.maxPlayers || !Main.player[NPC.target].active || Main.player[NPC.target].dead)
            {
                NPC.TargetClosest(true);
            }

            // 首次激活检查
            if (!isActive && Main.netMode != NetmodeID.MultiplayerClient)
            {
                ActivateBoss();
            }

            base.AI(); // 执行行为树

            // 更新冷却时间
            if (_dashCooldown > 0) _dashCooldown--;
            if (_summonCooldown > 0) _summonCooldown--;

            // 阶段切换：当血量低于50%时进入二阶段
            if (NPC.life <= NPC.lifeMax * 0.5f && currentPhase != 1)
            {
                ChangePhase(1);

                // 提示文本
                string message = Language.GetTextValue("Mods.AshenVoid.Content.NPCs.NightmareCorruption.Dialogue.Phase2");
                Main.NewText(message, Color.Purple);

                NPC.noGravity = false; // 二阶段受重力影响
                NPC.noTileCollide = false; // 二阶段有碰撞
            }

        }

        // ===== 具体行为实现 =====

        // 出生动画
        private NodeState PlayAppearingAnimation()
        {
            // Main.NewText($"PlayAppearingAnimation,Alpha: {NPC.alpha}");
            TargetIfRequired();
            var player = TargetPlayer;
            if (player == null)
            {
                return NodeState.Failure;
            }
            var pos = player.Center;
            pos.Y -= 270;
            NPC.Center = pos;
            if (NPC.alpha > 0)
            {
                NPC.alpha -= 4;
                return NodeState.Running;
            }
            else
            {
                NPC.alpha = 0;
                _isAppearing = false;
                PlaySound(new SoundStyle("AshenVoid/Assets/Sounds/Custom/NightmareCorruptionBorn"));
                // Main.NewText("PlayAppearingAnimationDone");
                return NodeState.Success;
            }
        }

        // 重写受伤方法以累计伤害
        public override void OnHitByItem(Player player, Item item, NPC.HitInfo hit, int damageDone)
        {
            _damageAccumulator += damageDone;
            SoundEngine.PlaySound(new SoundStyle("AshenVoid/Assets/Sounds/Custom/NightmareCorruptionHurt"), NPC.Center);
        }
        public override void OnHitByProjectile(Projectile projectile, NPC.HitInfo hit, int damageDone)
        {
            _damageAccumulator += damageDone;
            // 播放受伤音效
            SoundEngine.PlaySound(new SoundStyle("AshenVoid/Assets/Sounds/Custom/NightmareCorruptionHurt"), NPC.Center);
        }

    }
}
// using AshenVoid.Content.Items.Drops;
// using AshenVoid.Content.Projectiles;
// using AshenVoid.Core.BehaviorTree;
// using Microsoft.Xna.Framework;
// using System;
// using System.Collections.Generic;
// using Terraria;
// using Terraria.Audio;
// using Terraria.ID;
// using Terraria.Localization;
// using Terraria.ModLoader;

// namespace AshenVoid.Content.NPCs.NightmareCorruption
// {
//     [AutoloadBossHead]
//     public class NightmareCorruption : BossBase
//     {
//         // 状态变量
//         private bool _isAppearing = true;
//         private float _appearingProgress = 0f;
//         private float _damageAccumulator = 0f; // 用于冲刺触发的伤害累计
//         private bool _isSummoningMinions = false; // 是否正在召唤恍惚之握
//         private bool _isChasing = false; // 是否正在追击
//         private float _chaseSpeedMultiplier = 1f; // 追击速度倍率
//         private int _dashCooldown = 0; // 冲刺冷却
//         private int _summonCooldown = 0; // 召唤冷却
//         private int _deathTimer = 0; // 死亡动画计时器

//         // 幻影管理
//         private List<int> _phantoms = new List<int>(); // 存储幻影NPC的whoAmI

//         // 冲刺状态
//         private bool _isDashing = false;
//         private int _dashDuration = 0;
//         private Vector2 _dashDirection = Vector2.Zero;

//         // 下砸状态
//         private bool _isSlamPreparing = false;
//         private bool _isSlamming = false;

//         public override void SetDefaults()
//         {
//             NPC.width = 242;
//             NPC.height = 192;
//             NPC.lifeMax = 13100;
//             NPC.damage = 52;
//             NPC.defense = 10;
//             NPC.knockBackResist = 0f;
//             NPC.value = Item.buyPrice(0, 3, 0, 0);
//             NPC.boss = true;
//             NPC.noGravity = true;
//             NPC.noTileCollide = true;
//             NPC.netAlways = true;
//             NPC.aiStyle = -1;
//             NPC.alpha = 255;

//             NPC.HitSound = new SoundStyle("AshenVoid/Assets/Sounds/Custom/NightmareCorruptionHurt");
//             NPC.DeathSound = new SoundStyle("AshenVoid/Assets/Sounds/Custom/NightmareCorruptionDead");

//             Main.npcFrameCount[NPC.type] = 4;

//             Music = MusicLoader.GetMusicSlot(Mod, "Music/FoulAbyssEcho");
//         }

//         protected override void CreateBehaviorTree()
//         {
//             // 一阶段行为树
//             phaseBehaviors[0] = new FallbackNode(new List<Node>
//             {
//                 // 出生动画
//                 new ConditionNode(_isAppearing), // 检查是否在出生动画
//                 new ActionNode(PlayAppearingAnimation()),

//                 // 冲刺行为（伤害累计触发）
//                 new ConditionNode(_damageAccumulator >= 300),
//                 new ActionNode(StartDash()),

//                 // 召唤恍惚之握（每损失10%生命触发）
//                 new ConditionNode(NPC.life < NPC.lifeMax * 0.9f && !_isSummoningMinions && _summonCooldown <= 0),
//                 new ActionNode(SummonGraspOfTrance()),

//                 // 追击行为（距离大于650像素触发）
//                 new ConditionNode(Vector2.Distance(NPC.Center, TargetPlayer.Center) > 650),
//                 new ActionNode(StartChasePlayer()),

//                 // 冲刺行为处理
//                 new ConditionNode(_isDashing),
//                 new ActionNode(HandleDash()),

//                 // 徘徊行为
//                 new SequenceNode(new List<Node>
//                 {
//                     new ConditionNode(!_isChasing && !_isDashing), // 不在追击或冲刺时
//                     new ActionNode(WanderAbovePlayer()),
//                     new ActionNode(ShootProjectilesOnTurn()),
//                     new ConditionNode(Main.rand.NextBool(2) && _dashCooldown <= 0), // 50%概率且不在冷却
//                     new ActionNode(PrepareDash())
//                 }),
//             });

//             // 二阶段行为树
//             phaseBehaviors[1] = new SequenceNode(new List<Node>
//             {
//                 // 下砸攻击
//                 new ConditionNode(!_isSlamPreparing && !_isSlamming && !_isDashing),
//                 new ActionNode(PrepareSlam()),

//                 // 下砸处理
//                 new ConditionNode(_isSlamPreparing),
//                 new ActionNode(HandleSlamPreparation()),

//                 new ConditionNode(_isSlamming),
//                 new ActionNode(HandleSlam()),

//                 // 追击行为（距离大于2000像素或玩家高度大于700像素）
//                 new FallbackNode(new List<Node>
//                 {
//                     new ConditionNode(Vector2.Distance(NPC.Center, TargetPlayer.Center) > 2000),
//                     new ConditionNode(TargetPlayer.position.Y > 700 * 16) // 700像素（注意：position.Y是像素坐标）
//                 }),
//                 new ActionNode(TeleportAndSlam()),

//                 // 循环阶段：包围、狙击、冲刺
//                 new SequenceNode(new List<Node>
//                 {
//                     new ConditionNode(_phantoms.Count < 4 && NPC.life < NPC.lifeMax * (0.9f - _phantoms.Count * 0.1f)),
//                     new ActionNode(SummonPhantoms())
//                 }),

//                 new ConditionNode(_phantoms.Count > 0),
//                 new ActionNode(PhantomsSniping()),
//                 new ActionNode(PhantomsDashRotation()),

//                 // 冲刺行为处理
//                 new ConditionNode(_isDashing),
//                 new ActionNode(HandleDash())
//             });

//             // 初始化当前阶段行为树
//             currentPhase = 0;
//             BehaviorTree = phaseBehaviors[currentPhase];
//         }

//         public override void AI()
//         {
//             // 确保有目标玩家
//             if (NPC.target < 0 || NPC.target >= Main.maxPlayers || !Main.player[NPC.target].active || Main.player[NPC.target].dead)
//             {
//                 NPC.TargetClosest(true);
//             }

//             // 首次激活检查
//             if (!isActive && Main.netMode != NetmodeID.MultiplayerClient)
//             {
//                 ActivateBoss();
//             }

//             base.AI(); // 执行行为树

//             // 更新冷却时间
//             if (_dashCooldown > 0) _dashCooldown--;
//             if (_summonCooldown > 0) _summonCooldown--;

//             // 阶段切换：当血量低于50%时进入二阶段
//             if (NPC.life <= NPC.lifeMax * 0.5f && currentPhase != 1)
//             {
//                 ChangePhase(1);

//                 // 提示文本
//                 string message = Language.GetTextValue("Mods.AshenVoid.Content.NPCs.NightmareCorruption.Dialogue.Phase2");
//                 Main.NewText(message, Color.Purple);

//                 NPC.noGravity = false; // 二阶段受重力影响
//                 NPC.noTileCollide = false; // 二阶段有碰撞
//             }

//             // 死亡动画触发
//             if (NPC.life <= 1 && !NPC.dontTakeDamage)
//             {
//                 PlayDeathAnimation();
//                 NPC.dontTakeDamage = true; // 防止继续受到伤害
//             }

//             // 更新死亡动画
//             if (_deathTimer > 0)
//             {
//                 HandleDeathAnimation();
//             }
//         }

//         // ===== 具体行为实现 =====

//         // 出生动画
//         private NodeState PlayAppearingAnimation()
//         {
//             TargetIfRequired();
//             var player = TargetPlayer;
//             if (player == null)
//             {
//                 return NodeState.Failure;
//             }
//             var pos = player.Center;
//             pos.Y -= 270;
//             NPC.Center = pos;
//             if (NPC.alpha > 0)
//             {
//                 NPC.alpha -= 4;
//                 return NodeState.Running;
//             }
//             else
//             {
//                 NPC.alpha = 0;
//                 _isAppearing = false;
//                 PlaySound(new SoundStyle("AshenVoid/Assets/Sounds/Custom/NightmareCorruptionBorn"));
//                 Main.NewText("PlayAppearingAnimationDone");
//                 return NodeState.Success;
//             }
//         }

//         // 在玩家头顶徘徊
//         private NodeState WanderAbovePlayer()
//         {
//             if (TargetPlayer == null) return NodeState.Failure;

//             Main.NewText("执行徘徊行为");

//             Vector2 targetPos = TargetPlayer.Center + new Vector2(0, -200); // 玩家头顶200像素
//             MoveToPosition(targetPos);

//             // 左右移动逻辑
//             if (NPC.velocity.X == 0 || Main.GameUpdateCount % 120 == 0)
//             {
//                 NPC.velocity.X = Main.rand.NextBool() ? 3f : -3f;
//             }
//         }

//         // 折返时发射三叉弹幕
//         private NodeState ShootProjectilesOnTurn()
//         {
//             // 当速度方向改变时发射
//             if (Math.Sign(NPC.velocity.X) != Math.Sign(NPC.oldVelocity.X))
//             {
//                 if (TargetPlayer != null)
//                 {
//                     Vector2 direction = TargetPlayer.Center - NPC.Center;
//                     direction.Normalize();

//                     // 发射三叉弹幕，三个方向
//                     float spread = MathHelper.PiOver4 / 2;
//                     ShootProjectile(ModContent.ProjectileType<NightmareTrident>(), direction.RotatedBy(-spread), 8f, NPC.damage / 2);
//                     ShootProjectile(ModContent.ProjectileType<NightmareTrident>(), direction, 8f, NPC.damage / 2);
//                     ShootProjectile(ModContent.ProjectileType<NightmareTrident>(), direction.RotatedBy(spread), 8f, NPC.damage / 2);

//                     // 播放射击音效
//                     SoundEngine.PlaySound(new SoundStyle("AshenVoid/Assets/Sounds/Custom/NightmareCorruptionShoot"), NPC.Center);
//                 }
//             }
//         }

//         // 准备冲刺（向后移动并生成虚影）
//         private NodeState PrepareDash()
//         {
//             Vector2 direction = NPC.DirectionTo(TargetPlayer.Center);

//             // 确保最小距离320像素
//             if (Vector2.Distance(NPC.Center, TargetPlayer.Center) < 320)
//             {
//                 // 后退到安全距离
//                 NPC.velocity = -direction * 8f;
//                 return;
//             }

//             // 虚影由小变大效果
//             for (int i = 0; i < 5; i++)
//             {
//                 float scale = 0.5f + (i * 0.1f); // 逐渐变大
//                 Dust dust = Dust.NewDustPerfect(NPC.Center, DustID.Corruption, -direction * 2f, 0, default, scale);
//                 dust.noGravity = true;
//             }
//         }

//         // 开始冲刺
//         private NodeState StartDash()
//         {
//             if (TargetPlayer == null) return;

//             Main.NewText($"开始冲刺! 方向: {_dashDirection}, 速度: {NPC.velocity.Length()}px/tick");

//             _isDashing = true;
//             _dashDuration = 20; // 冲刺时间
//             _dashDirection = NPC.DirectionTo(TargetPlayer.Center);
//             NPC.velocity = _dashDirection * 20f;
//             _damageAccumulator = 0f; // 重置伤害累计
//         }

//         // 处理冲刺
//         private NodeState HandleDash()
//         {

//             Main.NewText($"冲刺中... 剩余时间: {_dashDuration} ticks");

//             _dashDuration--;

//             // 冲刺过程中生成腐化怪物
//             if (Main.netMode != NetmodeID.MultiplayerClient && Main.GameUpdateCount % 10 == 0)
//             {
//                 Vector2 spawnPos = NPC.Center + new Vector2(Main.rand.Next(-50, 50), Main.rand.Next(-50, 50));
//                 NPC.NewNPC(NPC.GetSource_FromAI(), (int)spawnPos.X, (int)spawnPos.Y, NPCID.Corruptor);
//             }

//             if (_dashDuration <= 0)
//             {
//                 _isDashing = false;
//                 NPC.velocity *= 0.5f; // 减速
//             }
//         }

//         // 召唤恍惚之握
//         private NodeState SummonGraspOfTrance()
//         {
//             if (TargetPlayer == null) return;

//             Main.NewText($"召唤恍惚之握 @ ({TargetPlayer.Center.X}, {TargetPlayer.Center.Y + 1000})");

//             _summonCooldown = 600; // 10秒冷却
//             _isSummoningMinions = true;

//             // 在目标下方1000像素处生成
//             Vector2 spawnPos = new Vector2(TargetPlayer.Center.X, TargetPlayer.Center.Y + 1000);
//             if (Main.netMode != NetmodeID.MultiplayerClient)
//             {
//                 int grasp = NPC.NewNPC(NPC.GetSource_FromAI(), (int)spawnPos.X, (int)spawnPos.Y, ModContent.NPCType<GraspOfTrance>());
//                 Main.npc[grasp].ai[0] = TargetPlayer.whoAmI; // 传递目标
//                 Main.npc[grasp].ai[1] = NPC.whoAmI; // 传递Boss
//             }
//         }

//         // 开始追击玩家
//         private NodeState StartChasePlayer()
//         {
//             _isChasing = true;
//             _chaseSpeedMultiplier = 3f; // 提升移速
//         }

//         // 准备下砸
//         private NodeState PrepareSlam()
//         {
//             if (TargetPlayer == null) return;

//             Main.NewText($"准备下砸! 跳跃高度: {TargetPlayer.Center.Y - NPC.Center.Y}px");

//             // 向高空跳跃
//             if (NPC.velocity.Y == 0) // 在地面上
//             {
//                 float height = MathHelper.Clamp(TargetPlayer.Center.Y - NPC.Center.Y, 300, 1000);
//                 NPC.velocity.Y = -(float)Math.Sqrt(2 * 0.3f * height);
//                 _isSlamPreparing = true;
//             }
//         }

//         // 处理下砸准备
//         private NodeState HandleSlamPreparation()
//         {
//             if (TargetPlayer == null) return;

//             // 水平移动到玩家头顶
//             if (Math.Abs(NPC.Center.X - TargetPlayer.Center.X) > 50)
//             {
//                 NPC.velocity.X = Math.Sign(TargetPlayer.Center.X - NPC.Center.X) * 10f;
//             }
//             else
//             {
//                 NPC.velocity.X = 0;
//                 NPC.velocity.Y = 0.5f; // 开始下落
//                 _isSlamPreparing = false;
//                 _isSlamming = true;
//             }
//         }

//         // 处理下砸
//         private NodeState HandleSlam()
//         {
//             // 下砸时加速
//             NPC.velocity.Y += 0.5f;

//             // 生成虚影在玩家移动方向前
//             if (Main.GameUpdateCount % 5 == 0 && TargetPlayer != null)
//             {
//                 Vector2 direction = new Vector2(TargetPlayer.velocity.X, 0).SafeNormalize(Vector2.UnitX);
//                 if (direction == Vector2.Zero) direction = Vector2.UnitX;

//                 // 生成虚影
//                 Dust dust = Dust.NewDustPerfect(TargetPlayer.Center + direction * 100, DustID.Corruption, Vector2.Zero);
//                 dust.noGravity = true;
//                 dust.scale = 1.5f;

//                 // 发射弹幕
//                 ShootProjectile(ModContent.ProjectileType<NightmareTrident>(), direction, 10f, NPC.damage / 2);
//             }

//             // 落地检测
//             if (NPC.velocity.Y == 0)
//             {
//                 _isSlamming = false;

//                 // 冲击波效果
//                 for (int i = 0; i < 20; i++)
//                 {
//                     Vector2 dustPos = NPC.Bottom + new Vector2(Main.rand.Next(-50, 50), 0);
//                     Dust.NewDust(dustPos, 10, 10, DustID.Corruption, 0f, -5f, 0, default, 2f);
//                 }
//             }
//         }

//         // 二阶段：传送并下砸
//         private NodeState TeleportAndSlam()
//         {
//             if (TargetPlayer == null) return;

//             Main.NewText($"传送至玩家头顶 @ ({TargetPlayer.Center.X}, {TargetPlayer.Center.Y - 300})");

//             // 传送至玩家头顶
//             NPC.Center = TargetPlayer.Center + new Vector2(0, -300);
//             NPC.alpha = 255; // 完全透明
//             NPC.netUpdate = true;

//             // 透明度逐渐降低（显现）
//             if (NPC.alpha > 0)
//             {
//                 NPC.alpha -= 10;
//             }
//             else
//             {
//                 // 显现完成后立即下砸
//                 PrepareSlam(npc);
//             }
//         }

//         // 二阶段：召唤幻影
//         private NodeState SummonPhantoms()
//         {

//             if (Main.netMode != NetmodeID.MultiplayerClient)
//             {
//                 Main.NewText($"召唤噩梦幻影 (当前数量: {_phantoms.Count + 1})");

//                 int phantom = NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.Center.X, (int)NPC.Center.Y, ModContent.NPCType<NightmarePhantom>());
//                 _phantoms.Add(phantom);
//                 // 设置幻影的主人
//                 Main.npc[phantom].ai[0] = NPC.whoAmI;
//                 Main.npc[phantom].ai[1] = _phantoms.Count - 1; // 索引，用于确定位置
//             }
//         }

//         // 二阶段：幻影狙击
//         private NodeState PhantomsSniping()
//         {
//             // 让幻影回归本体周围
//             for (int i = 0; i < _phantoms.Count; i++)
//             {
//                 int phantomIndex = _phantoms[i];
//                 if (Main.npc[phantomIndex].active)
//                 {
//                     Vector2 offset = new Vector2(100, 0).RotatedBy(MathHelper.TwoPi * i / _phantoms.Count);
//                     MoveNPCToPosition(Main.npc[phantomIndex], NPC.Center + offset);
//                 }
//             }

//             // 本体与幻影同步快速射击
//             if (Main.GameUpdateCount % 30 == 0 && TargetPlayer != null)
//             {
//                 Vector2 direction = NPC.DirectionTo(TargetPlayer.Center);
//                 ShootProjectile(ModContent.ProjectileType<NightmareTrident>(), direction, 12f, NPC.damage / 3);

//                 foreach (int phantomIndex in _phantoms)
//                 {
//                     if (Main.npc[phantomIndex].active)
//                     {
//                         direction = Main.npc[phantomIndex].DirectionTo(TargetPlayer.Center);
//                         Projectile.NewProjectile(NPC.GetSource_FromAI(), Main.npc[phantomIndex].Center,
//                             direction * 12f, ModContent.ProjectileType<NightmareTrident>(), NPC.damage / 3, 0f, Main.myPlayer);
//                     }
//                 }

//                 // 播放射击音效
//                 SoundEngine.PlaySound(new SoundStyle("AshenVoid/Assets/Sounds/Custom/NightmareCorruptionShoot"), NPC.Center);
//             }
//         }

//         // 二阶段：幻影冲刺
//         private NodeState PhantomsDashRotation()
//         {
//             // 幻影呈正方形排列绕本体旋转
//             for (int i = 0; i < _phantoms.Count; i++)
//             {
//                 int phantomIndex = _phantoms[i];
//                 if (Main.npc[phantomIndex].active)
//                 {
//                     float angle = MathHelper.TwoPi * i / _phantoms.Count + Main.GameUpdateCount * 0.05f;
//                     Vector2 offset = new Vector2(200, 0).RotatedBy(angle);
//                     MoveNPCToPosition(Main.npc[phantomIndex], NPC.Center + offset);
//                 }
//             }

//             // 本体向玩家冲刺
//             if (TargetPlayer != null)
//             {
//                 Vector2 direction = NPC.DirectionTo(TargetPlayer.Center);
//                 NPC.velocity = direction * 15f;
//                 _isDashing = true;
//                 _dashDuration = 40;
//             }
//         }

//         // 辅助方法：移动NPC到目标位置
//         private NodeState MoveNPCToPosition(NPC npc, Vector2 targetPos, float maxSpeed = 15f, float acceleration = 0.5f)
//         {
//             Vector2 direction = targetPos - npc.Center;
//             if (direction != Vector2.Zero)
//             {
//                 direction.Normalize();
//             }
//             npc.velocity = npc.velocity * (1 - acceleration) + direction * maxSpeed * acceleration;
//             if (npc.velocity.Length() > maxSpeed)
//             {
//                 npc.velocity = npc.velocity.SafeNormalize(Vector2.Zero) * maxSpeed;
//             }
//         }

//         // 死亡动画
//         private NodeState PlayDeathAnimation()
//         {
//             _deathTimer = 1;
//             NPC.life = 1;
//             NPC.dontTakeDamage = true;
//             NPC.velocity = Vector2.Zero;

//             // 若在空中则自由落体至地面
//             if (NPC.noGravity)
//             {
//                 NPC.noGravity = false;
//                 NPC.velocity.Y = 5f;
//             }

//             // 播放死亡音效
//             SoundEngine.PlaySound(new SoundStyle("AshenVoid/Assets/Sounds/Custom/NightmareCorruptionDead"), NPC.Center);
//         }

//         private NodeState HandleDeathAnimation()
//         {
//             _deathTimer++;

//             // 虚影抖动范围逐渐扩大
//             if (_deathTimer < 60)
//             {
//                 NPC.position += new Vector2(Main.rand.Next(-2, 3), Main.rand.Next(-2, 3));
//             }

//             // 本体明度持续降低直至全黑（持续3秒）
//             float progress = MathHelper.Clamp(_deathTimer / 180f, 0f, 1f);
//             NPC.color = Color.Lerp(Color.White, Color.Black, progress);

//             // 虚影聚拢至本体（后60帧）
//             if (_deathTimer > 120 && _deathTimer < 180)
//             {
//                 // 聚拢效果
//                 for (int i = 0; i < 5; i++)
//                 {
//                     Vector2 dustPos = NPC.Center + new Vector2(Main.rand.Next(-200, 200), Main.rand.Next(-200, 200));
//                     Vector2 dustVel = NPC.DirectionFrom(dustPos) * 5f;
//                     Dust.NewDust(dustPos, 10, 10, DustID.Corruption, dustVel.X, dustVel.Y, 0, default, 2f);
//                 }
//             }

//             // 本体原地升空剧烈抖动（180-240帧）
//             if (_deathTimer > 180 && _deathTimer < 240)
//             {
//                 NPC.velocity.Y = -3f;
//                 NPC.position += new Vector2(Main.rand.Next(-5, 6), Main.rand.Next(-1, 2));
//             }

//             // 空中碎裂（240帧）
//             if (_deathTimer >= 240)
//             {
//                 // 生成碎裂特效
//                 for (int i = 0; i < 50; i++)
//                 {
//                     Vector2 vel = new Vector2(Main.rand.NextFloat(-5, 5), Main.rand.NextFloat(-5, 5));
//                     Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Corruption, vel.X, vel.Y, 0, default, 2f);
//                 }

//                 // 生成尸体
//                 if (Main.netMode != NetmodeID.MultiplayerClient)
//                 {
//                     NPC.NewNPC(NPC.GetSource_Death(), (int)NPC.Center.X, (int)NPC.Center.Y, ModContent.NPCType<NightmareCorruptionCorpse>());
//                 }

//                 // 掉落物品
//                 Item.NewItem(NPC.GetSource_Loot(), NPC.Center, ModContent.ItemType<NightmareEssence>(), Main.rand.Next(5, 10));

//                 // 结束NPC
//                 NPC.active = false;
//             }
//         }

//         // 重写受伤方法以累计伤害
//         public override void OnHitByItem(Player player, Item item, NPC.HitInfo hit, int damageDone)
//         {
//             _damageAccumulator += damageDone;
//             SoundEngine.PlaySound(new SoundStyle("AshenVoid/Assets/Sounds/Custom/NightmareCorruptionHurt"), NPC.Center);
//         }
//         public override void OnHitByProjectile(Projectile projectile, NPC.HitInfo hit, int damageDone)
//         {
//             _damageAccumulator += damageDone;
//             // 播放受伤音效
//             SoundEngine.PlaySound(new SoundStyle("AshenVoid/Assets/Sounds/Custom/NightmareCorruptionHurt"), NPC.Center);
//         }

//     }
// }