using System.IO;

namespace AshenVoid.Core.Events
{
    /// <summary>
    /// Represents an event that can be synchronized over the network.
    /// </summary>
    public interface INetworkEvent : IEvent
    {
        /// <summary>
        /// Writes the event data to a binary writer for network transmission.
        /// </summary>
        void Write(BinaryWriter writer);

        /// <summary>
        /// Reads the event data from a binary reader on the receiving end.
        /// </summary>
        void Read(BinaryReader reader);
    }
}