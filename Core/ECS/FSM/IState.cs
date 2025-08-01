using AshenVoid.Core.ECS.AI;
using System;

namespace AshenVoid.Core.ECS.FSM
{
    /// <summary>
    /// 定义一个无状态的有限状态机（FSM）状态。
    /// 状态类本身不应包含任何随时间变化的数据。所有实例数据都应存储在Blackboard中。
    /// 这使得单个状态实例可以被多个NPC安全地共享。
    /// </summary>
    public interface IState
    {
        /// <summary>
        /// 当状态机进入此状态时调用一次。
        /// 用于在Blackboard中初始化此状态所需的数据（例如计时器、计数器）。
        /// </summary>
        void Enter(Blackboard blackboard);

        /// <summary>
        /// 当此状态处于活动状态时，每帧调用。
        /// 这是执行状态逻辑的地方。
        /// <returns>如果需要转换到新状态，则返回新状态的类型；否则返回null。</returns>
        /// </summary>
        Type Update(Blackboard blackboard);

        /// <summary>
        /// 当状态机离开此状态时调用一次。
        /// 用于从Blackboard中清理此状态设置的数据（如果需要）。
        /// </summary>
        void Exit(Blackboard blackboard);
    }
}