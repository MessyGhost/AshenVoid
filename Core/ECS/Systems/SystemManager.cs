using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;

namespace AshenVoid.Core.ECS.Systems
{
    public class SystemManager
    {
        private readonly List<ISystem> _systems = new List<ISystem>();

        public void RegisterSystem(ISystem system)
        {
            _systems.Add(system);
        }

        public void Update(GameTime gameTime, NPC npc)
        {
            foreach (var system in _systems)
            {
                system.Update(gameTime, npc);
            }
        }
    }
}