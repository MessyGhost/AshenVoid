namespace AshenVoid.Content.NPCs.NightmareCorruption.Configs
{
    // Parameters for MovementComponent and related BT nodes
    public class MovementStats
    {
        // Second-Order Dynamics PID parameters
        public float Frequency { get; set; }
        public float DampingRatio { get; set; }
        public float ResponseScale { get; set; }

        // Behavior Tree node parameters
        public float ChaseDistanceFar { get; set; } // 当玩家超过这个距离时，强制追击
        public float ChaseStopDistance { get; set; } // 追击时与玩家保持的距离
        public float OrbitRadius { get; set; } // 环绕玩家的半径
        public float ThinkIntervalMin { get; set; } // 思考间隔（秒）
        public float ThinkIntervalMax { get; set; } // 思考间隔（秒）
        public float MaxSpeed { get; set; } // 移动速度
    }
}