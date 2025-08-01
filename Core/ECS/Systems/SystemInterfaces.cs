using AshenVoid.Core.Events;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;

namespace AshenVoid.Core.ECS.Systems
{
    public enum SystemExecutionSide
    {
        Both,
        Server,
        Client
    }

    public interface ISystem { }

    public interface IComponentSystem : ISystem
    {
        IEnumerable<Type> RequiredComponents { get; }
        SystemExecutionSide ExecutionSide { get; }
        void Update(GameTime gameTime, int entityId, EcsWorld world, EventBus eventBus);
    }
}