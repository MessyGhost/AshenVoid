using AshenVoid.Core.ECS.Systems;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using Terraria;

namespace AshenVoid.Core.ECS
{
    public class ComponentController
    {
        private readonly List<IComponent> _components = new List<IComponent>();
        private readonly SystemManager _systemManager = new SystemManager();

        public ComponentController()
        {
        }

        public void RegisterSystem(ISystem system)
        {
            _systemManager.RegisterSystem(system);
        }

        public void RegisterComponent(IComponent component)
        {
            _components.Add(component);
        }

        public T GetComponent<T>() where T : class
        {
            return _components.OfType<T>().FirstOrDefault();
        }

        public void Update(GameTime gameTime, NPC npc)
        {
            foreach (var component in _components)
            {
                component.Update();
            }
            _systemManager.Update(gameTime, npc);
        }
    }
}