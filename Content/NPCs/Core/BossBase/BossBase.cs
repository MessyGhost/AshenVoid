
using AshenVoid.Core.BehaviorTree;
using AshenVoid.Core.Processor;
using AshenVoid.Core.Processor.AshenVoid.Core.FX;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;

namespace AshenVoid.Content.NPCs
{
    public abstract partial class BossBase : EnemyBase
    {
        // 阶段管理
        protected int currentPhase = 0;
        protected Dictionary<int, Node> phaseBehaviors = new Dictionary<int, Node>();

        // Boss状态
        protected bool isActive = false;
        protected bool hasSummonedMinions = false;

        // 移动系统
        protected SecondOrderDynamics _movementController;
        protected Vector2 outputPos;            // 系统输出坐标
        protected Vector2 snapshotPos;          // 用于保存玩家位置的快照
        protected Vector2 _currentTargetPos;      // 当前目标位置
        public Vector2 velocity;                // 计算得到的速度，用于倾斜动画

        // 行为树
        protected Node BehaviorTree;

        // 创建行为树（子类boss行为在这里编写）
        protected abstract void CreateBehaviorTree();

        // 效果器
        protected ProcessorChain _chain = new ProcessorChain();

        // 创建效果器链（子类boss的效果在这里编写，注意将超类方法写开头）
        protected virtual void createProcessorChain()
        {
            // 添加通用的FX (帧动画、倾斜)
            _chain.Add(new Transformer());
            _chain.Add(new Framer());
            _chain.Add(new Tilter(velocity));
        }

        // 切换Boss阶段
        public virtual void ChangePhase(int newPhase)
        {
            if (phaseBehaviors.TryGetValue(newPhase, out Node newBehavior))
            {
                Main.NewText($"阶段切换: {currentPhase} -> {newPhase}");
                currentPhase = newPhase;
                BehaviorTree = newBehavior;
            }
        }

        // boss出生时调用
        protected virtual void ActivateBoss()
        {
            // 初始化Fx
            _chain = new ProcessorChain();

            // 初始化控制器
            _movementController = new SecondOrderDynamics();
            _movementController.Init(NPC.Center);
            _currentTargetPos = NPC.Center;

            // 创造行为树
            CreateBehaviorTree();

            // 创造效果链
            createProcessorChain();

            // 激活boss
            isActive = true;
            NPC.TargetClosest(true);
            NPC.netUpdate = true;

            // 出生阶段
            ChangePhase(0);
        }

        // 帧循环逻辑
        public override void AI()
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;

            // 首次激活检查
            if (!isActive) ActivateBoss();

            // 效果器链pre处理
            _chain.ProcessPreAI(NPC);

            // 使用行为树进行决策
            BehaviorTree?.Evaluate();

            // 使用pid系统更新位置
            UpdateMovement();

            // Main.NewText(NPC.position);

            // 效果器链pos处理
            _chain.ProcessPostAI(NPC);

            // Main.NewText(NPC.position);
        }

        protected virtual void UpdateMovement()
        {
            outputPos = _movementController.Update(
                (float)Main.gameTimeCache.ElapsedGameTime.TotalSeconds,
                _currentTargetPos
            );
            // 计算速度差值
            velocity = (outputPos - NPC.Center) / (float)Main.gameTimeCache.ElapsedGameTime.TotalSeconds;
            NPC.Center = outputPos;
        }

        public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            _chain.ProcessPostDraw(NPC, spriteBatch, screenPos, drawColor);
            base.PostDraw(spriteBatch, screenPos, drawColor);
        }

        public override void FindFrame(int frameHeight)
        {
            _chain.ProcessFindFrame(NPC);
            // 这里为了统一的架构，把帧动画播放当作基础Fx了，所以我们不需要调用父类方法
            // enemybase将会在后续重写
            // base.FindFrame(frameHeight);
        }
    }
}