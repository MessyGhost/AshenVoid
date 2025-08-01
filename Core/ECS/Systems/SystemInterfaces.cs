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
}