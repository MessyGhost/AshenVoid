using Microsoft.Xna.Framework;

namespace AshenVoid.Core.ECS.Intents
{
    /// <summary>
    /// Represents the AI's desire for movement.
    /// This is a data container, holding no logic itself. The MovementComponent will interpret this intent.
    /// </summary>
    public interface IMovementIntent { }

    /// <summary>
    /// Intent to move towards a specific position.
    /// </summary>
    public class ChaseIntent : IMovementIntent
    {
        public Vector2 TargetPosition { get; }
        public float StopDistance { get; }

        public ChaseIntent(Vector2 targetPosition, float stopDistance = 0f)
        {
            TargetPosition = targetPosition;
            StopDistance = stopDistance;
        }
    }

    /// <summary>
    /// Intent to orbit around a central point.
    /// </summary>
    public class OrbitIntent : IMovementIntent
    {
        public Vector2 Center { get; }
        public float Radius { get; }
        public int Direction { get; } // 1 for clockwise, -1 for counter-clockwise

        public OrbitIntent(Vector2 center, float radius, int direction = 1)
        {
            Center = center;
            Radius = radius;
            Direction = direction;
        }
    }

    /// <summary>
    /// Intent to move away from a specific position.
    /// </summary>
    public class FleeIntent : IMovementIntent
    {
        public Vector2 FleeFromPosition { get; }

        public FleeIntent(Vector2 fleeFromPosition)
        {
            FleeFromPosition = fleeFromPosition;
        }
    }

    /// <summary>
    /// Intent to do nothing, allowing inertia to take over or to simply stop.
    /// </summary>
    public class IdleIntent : IMovementIntent { }
}