using System;
using AshenVoid.Core.BehaviorTree;
using Microsoft.Xna.Framework;
using Terraria;
using static AshenVoid.Core.BehaviorTree.NodeBuilder;
namespace AshenVoid.Content.NPCs
{
    public abstract partial class BossBase
    {
        #region 移动预设
        /// <summary>
        /// 简单的接近
        /// </summary>
        protected Node Approach(Func<Vector2> target)
        {
            return TickMoveTo(target, 0.5f, 0.9f);
        }

        /// <summary>
        /// 简单的追及
        /// </summary>
        protected Node Chase(Func<Vector2> target)
        {
            // Main.NewText("chase");
            return TickMoveTo(target, 1f, 1.1f);
        }

        /// <summary>
        /// 简单的环绕
        /// </summary>
        protected Node OrbitAround(Func<Vector2> center, float radius = 250f, float speed = 3f)
        {
            return Do(() =>
            {
                float angle = (float)Main.gameTimeCache.TotalGameTime.TotalSeconds * speed;
                Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius;
                _currentTargetPos = center() + offset;
            });
        }

        /// <summary>
        /// 定点接近
        /// </summary>
        protected Node OnceApproach(Func<Vector2> target, float stopDistance = 100f)
        {
            return OnceMoveTo(target, stopDistance,
                0.5f, 0.5f);
        }

        /// <summary>
        /// 在指定范围内随机定点接近
        /// </summary>
        private Vector2? wanderTarget;
        protected Node OnceRandomApproach(Func<Vector2> center, float radius = 150f)
        {
            return Sequence(
                Once(
                    () =>
                    {
                        // 生成新的随机目标点
                        float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                        float distance = Main.rand.NextFloat(radius * 0.05f, radius);
                        wanderTarget = center() + new Vector2(
                            (float)Math.Cos(angle) * distance,
                            (float)Math.Sin(angle) * distance
                        );
                    }
                ),
                OnceMoveTo(() => wanderTarget.Value, 15f, 0.5f, 0.3f, 0)
            );
        }

        /// <summary>
        /// 快速冲刺移动
        /// </summary>
        protected Node RushTowards(Func<Vector2> target, float stopDistance = 100f)
        {
            return OnceMoveTo(() => target(), stopDistance,
                1.5f, 0.95f, -0.5f);
        }

        /// <summary>
        /// 蓄力冲刺移动
        /// </summary>
        protected Node RushTowardsWithBuildUp(Func<Vector2> target, float stopDistance = 50f)
        {
            return
            Parallel(
                Sequence(WaitSeconds(1f), RushTowardsWithStartUp(() => target(), stopDistance)),
                Breathe(2f)
                );
        }

        /// <summary>
        /// 带前摇的快速冲刺移动
        /// </summary>
        protected Node RushTowardsWithStartUp(Func<Vector2> target, float stopDistance = 100f)
        {
            return OnceMoveTo(() => target(), stopDistance,
                1.0f, 0.95f, -0.5f);
        }

        #endregion 移动预设

        #region 移动节点

        // 基于pid二阶系统的移动方法，传入targetpos产生阶跃，可修改系统相关函数
        // 注意该方法没有终止条件，使用时需要配合高优先级抢断或条件判断节点
        protected Node TickMoveTo(Func<Vector2> target,
                                        float f = 0.5f,
                                        float z = 1.0f,
                                        float r = 0)
        {
            return Do(() =>
            {
                _currentTargetPos = target();
                // Main.NewText($"TickMoveTo:{_currentTargetPos}");
                _movementController.SetConstants(f, z, r);
                return NodeState.Success;
            });
        }

        // 只获取一次目标位置的移动，适用于冲刺等行为
        protected Node OnceMoveTo(Func<Vector2> target,
                                        float stopDistance = 10f,
                                        float f = 0.5f,
                                        float z = 1.0f,
                                        float r = 0)
        {
            return Once(
                Sequence(
                    Once(
                        () =>
                        {
                            _currentTargetPos = target();
                            _movementController.SetConstants(f, z, r);
                            // Main.NewText($"GetTarget:{_currentTargetPos}");
                        }
                    ),
                    Do(() =>
                    {
                        if (stopDistance > (_currentTargetPos - NPC.Center).Length())
                        {
                            // Main.NewText($"Approach: {_currentTargetPos}");
                            return NodeState.Success;
                        }
                        return NodeState.Running;
                    }
                    )
                )
            );
        }


        #endregion 移动节点
    }
}
