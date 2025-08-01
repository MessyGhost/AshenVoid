using AshenVoid.Core.Builders;
using AshenVoid.Core.ECS;
using AshenVoid.Core.ECS.FSM;
using AshenVoid.Core.Events;
using System;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace AshenVoid
{
	public class AshenVoid : Mod
	{
		public enum MessageType : byte
		{
			SyncBossState,
			SyncNetworkEvent
		}

		public override void HandlePacket(BinaryReader reader, int whoAmI)
		{
			MessageType msgType = (MessageType)reader.ReadByte();

			switch (msgType)
			{
				case MessageType.SyncBossState:
					HandleStateSync(reader);
					break;
                
                case MessageType.SyncNetworkEvent:
                    HandleNetworkEventSync(reader);
                    break;
			}
		}

		private void HandleStateSync(BinaryReader reader)
		{
			if (Main.netMode != NetmodeID.MultiplayerClient) return;

			byte npcId = reader.ReadByte();
			byte stateId = reader.ReadByte();

			NPC npc = Main.npc[npcId];
			if (npc.active && npc.ModNPC is EcsBoss boss)
			{
				var aiState = boss.Controller.GetComponent<AIStateComponent>();
				var stateType = aiState.GetStateType(stateId);
				if (stateType != null)
				{
					var services = aiState.Blackboard.Get<Core.ServiceLocator>(Core.ECS.AI.BlackboardKeys.ServiceLocator);
					var stateFactory = services.Get<StateFactory>();
					var newState = stateFactory.GetState(stateType);
					
					aiState.StateMachine.ReceiveStateChange(newState, aiState.Blackboard);
				}
			}
		}

		private void HandleNetworkEventSync(BinaryReader reader)
		{
			if (Main.netMode != NetmodeID.MultiplayerClient) return;

			byte npcId = reader.ReadByte();
			string eventTypeName = reader.ReadString();
			
			NPC npc = Main.npc[npcId];
			if (npc.active && npc.ModNPC is EcsBoss boss)
			{
				var eventType = Type.GetType(eventTypeName);
				if (eventType != null && Activator.CreateInstance(eventType) is INetworkEvent networkEvent)
				{
					networkEvent.Read(reader);
					// Re-publish the event on the client's event bus
					var eventBus = boss.Services.Get<EventBus>();
					eventBus.Publish(networkEvent);
				}
			}
		}
	}
}