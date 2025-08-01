using System.IO;
using Terraria;

namespace AshenVoid.Core.ECS.Interfaces
{
    /// <summary>
    /// Defines a component whose state can be synchronized over the network.
    /// </summary>
    public interface INetworkedComponent : IComponent
    {
        /// <summary>
        /// Writes the component's necessary state to the network stream.
        /// Called on the server side.
        /// </summary>
        /// <param name="npc">The NPC instance.</param>
        /// <param name="writer">The binary writer to write data to.</param>
        void SendData(NPC npc, BinaryWriter writer);

        /// <summary>
        /// Reads the component's state from the network stream.
        /// Called on the client side.
        /// </summary>
        /// <param name="npc">The NPC instance.</param>
        /// <param name="reader">The binary reader to read data from.</param>
        void ReceiveData(NPC npc, BinaryReader reader);
    }
}