using System.IO;
using Terraria;

namespace AshenVoid.Core.Events
{
    /// <summary>
    /// Published when an NPC is damaged.
    /// Systems can listen to this to react to damage, e.g., by changing stats or behavior.
    /// The ComponentController can be accessed via the NPC instance if it's an EcsBoss.
    /// </summary>
    public readonly struct NPCDamagedEvent : IEvent
    {
        public readonly NPC NPC;
        public readonly NPC.HitInfo Hit;

        public NPCDamagedEvent(NPC npc, NPC.HitInfo hit)
        {
            NPC = npc;
            Hit = hit;
        }
    }

    /// <summary>
    /// Published by the HealthSystem when an NPC's health changes.
    /// </summary>
    public readonly struct NPCHealthLossEvent : IEvent
    {
        public readonly NPC NPC;
        public readonly float HealthPercentage;
        public readonly float PreviousHealthPercentage;

        public NPCHealthLossEvent(NPC npc, float healthPercentage, float previousHealthPercentage)
        {
            NPC = npc;
            HealthPercentage = healthPercentage;
            PreviousHealthPercentage = previousHealthPercentage;
        }
    }

    /// <summary>
    /// Network event to synchronize an ECS entity's ID with a Terraria NPC's ID.
    /// Sent from the server to all clients when an EcsBoss is spawned.
    /// </summary>
    public struct EntityIdSyncEvent : INetworkEvent
    {
        public int NpcWhoAmI;
        public int EntityId;

        public EntityIdSyncEvent(int npcWhoAmI, int entityId)
        {
            NpcWhoAmI = npcWhoAmI;
            EntityId = entityId;
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write((byte)NpcWhoAmI);
            writer.Write(EntityId);
        }

        public void Read(BinaryReader reader)
        {
            NpcWhoAmI = reader.ReadByte();
            EntityId = reader.ReadInt32();
        }
    }

    /// <summary>
    /// Network event to inform clients that an entity's AI state has changed.
    /// </summary>
    public struct StateChangedNetworkEvent : INetworkEvent
    {
        public int EntityId { get; set; }
        public byte StateId { get; set; }

        public void Write(BinaryWriter writer)
        {
            writer.Write(EntityId);
            writer.Write(StateId);
        }

        public void Read(BinaryReader reader)
        {
            EntityId = reader.ReadInt32();
            StateId = reader.ReadByte();
        }
    }

    /// <summary>
    /// Network event to inform clients that an entity has performed an attack.
    /// This can be used to trigger VFX and SFX on the client.
    /// </summary>
    public struct AttackPerformedNetworkEvent : INetworkEvent
    {
        public int EntityId;
        public string AttackName; // An identifier for the type of attack

        public void Write(BinaryWriter writer)
        {
            writer.Write(EntityId);
            writer.Write(AttackName);
        }

        public void Read(BinaryReader reader)
        {
            EntityId = reader.ReadInt32();
            AttackName = reader.ReadString();
        }
    }
}