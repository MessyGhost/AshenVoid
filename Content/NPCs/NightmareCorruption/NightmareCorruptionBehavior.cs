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

        private Node CreatePhase1Behavior()
        {
            return new RepeatNode(
                new SequenceNode(
                    new ActionNode(() => MoveToTarget(() => TargetPlayer.Center + new Vector2(-120, -200))),
                    new WaitFramesNode(60), // 停留1秒
                    new ActionNode(() => MoveToTarget(() => TargetPlayer.Center + new Vector2(120, -200))),
                    new WaitFramesNode(60), // 停留1秒
                    new ActionNode(() => MoveToTarget(() => TargetPlayer.Center + new Vector2(-120, -200))),
                    new WaitFramesNode(60), // 停留1秒
                    new ActionNode(() => MoveToTarget(() => TargetPlayer.Center + new Vector2(120, -200))),
                    new WaitFramesNode(60) // 停留1秒
                ),
                -1 // 负数代表无限重复
            );
        }

        private Node CreatePhase2Behavior()
        {
            return new ParallelNode(
                // 主行为：持续追逐
                new ActionNode(() => ChaseTarget(80f)),

                // 辅助行为：周期召唤小弟
                new SequenceNode(
                    // 每10秒召唤
                    new WaitFramesNode(600),

                    // 召唤2个幻影
                    new ActionNode(() => SummonMinions(ModContent.NPCType<NightmarePhantom>(), 2))
                )
            );
        }

        public override void OnHitByItem(Player player, Item item, NPC.HitInfo hit, int damageDone)
        {
            base.OnHitByItem(player, item, hit, damageDone);
            SoundEngine.PlaySound(new SoundStyle("AshenVoid/Assets/Sounds/Custom/NightmareCorruptionHurt"), NPC.Center);
        }

        public override void OnHitByProjectile(Projectile projectile, NPC.HitInfo hit, int damageDone)
        {
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