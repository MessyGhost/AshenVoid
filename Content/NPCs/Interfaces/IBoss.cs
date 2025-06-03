using Microsoft.Xna.Framework;
using Terraria;

namespace AshenVoid.Content.NPCs
{
    public interface IBossState
    {
        void Enter();
        void Update();
        void Exit();
    }

    public interface IBossBehavior
    {
        void Execute();
    }

    public interface IBossEntity
    {
        NPC NPC { get; }
        Vector2 Position { get; set; }
        int Health { get; set; }
        int MaxHealth { get; }
        int Target { get; set; }

        BossStateMachine StateMachine { get; }
        void AddBehavior(IBossBehavior behavior);
        void RemoveBehavior<T>() where T : IBossBehavior;
    }

    public class BossStateMachine
    {
        private IBossState _currentState;
        private readonly IBossEntity _boss;

        public BossStateMachine(IBossEntity boss)
        {
            _boss = boss;
        }

        public void ChangeState(IBossState newState)
        {
            _currentState?.Exit();
            _currentState = newState;
            _currentState?.Enter();
        }

        public void Update()
        {
            _currentState?.Update();
        }
    }

    // 基础行为模块
    public abstract class BaseBehavior : IBossBehavior
    {
        protected readonly IBossEntity Boss;

        protected BaseBehavior(IBossEntity boss)
        {
            Boss = boss;
        }

        public abstract void Execute();
    }

    // 追逐玩家行为
    public class ChasePlayerBehavior : BaseBehavior
    {
        private readonly float _speed;

        public ChasePlayerBehavior(IBossEntity boss, float speed) : base(boss)
        {
            _speed = speed;
        }

        public override void Execute()
        {
            Player target = Main.player[Boss.Target];
            if (target == null || !target.active) return;

            Vector2 direction = (target.Center - Boss.Position).SafeNormalize(Vector2.Zero);
            Boss.NPC.velocity = Vector2.Lerp(Boss.NPC.velocity, direction * _speed, 0.05f);
        }
    }

    // 发射弹幕行为
    public class ShootProjectilesBehavior : BaseBehavior
    {
        private readonly int _projectileType;
        private readonly float _speed;
        private readonly int _count;
        private readonly float _spread;

        public ShootProjectilesBehavior(IBossEntity boss, int projectileType, float speed, int count, float spread)
            : base(boss)
        {
            _projectileType = projectileType;
            _speed = speed;
            _count = count;
            _spread = spread;
        }

        public override void Execute()
        {
            if (Main.netMode == Terraria.ID.NetmodeID.MultiplayerClient) return;

            Player target = Main.player[Boss.Target];
            if (target == null || !target.active) return;

            Vector2 baseDirection = (target.Center - Boss.Position).SafeNormalize(Vector2.Zero);

            for (int i = 0; i < _count; i++)
            {
                Vector2 velocity = baseDirection.RotatedBy(_spread * (i - (_count - 1) / 2f)) * _speed;
                Projectile.NewProjectile(Boss.NPC.GetSource_FromAI(), Boss.Position, velocity,
                    _projectileType, Boss.NPC.damage / 2, 0f);
            }
        }
    }

    // 召唤小怪行为
    public class SummonMinionsBehavior : BaseBehavior
    {
        private readonly int[] _minionTypes;
        private readonly int _weightLimit;

        public SummonMinionsBehavior(IBossEntity boss, int[] minionTypes, int weightLimit) : base(boss)
        {
            _minionTypes = minionTypes;
            _weightLimit = weightLimit;
        }

        public override void Execute()
        {
            if (Main.netMode == Terraria.ID.NetmodeID.MultiplayerClient) return;

            int weight = 0;
            while (weight < _weightLimit)
            {
                int type = _minionTypes[Main.rand.Next(_minionTypes.Length)];
                NPC.NewNPCDirect(Boss.NPC.GetSource_FromAI(), Boss.Position, type);
                weight += GetMinionWeight(type);
            }
        }

        private int GetMinionWeight(int type)
        {
            // 根据小怪类型返回权重
            return type == Terraria.ID.NPCID.DevourerHead ? 2 : 1;
        }
    }
}