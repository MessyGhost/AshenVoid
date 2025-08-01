using AshenVoid.Core.ECS;
using AshenVoid.Core.ECS.FSM;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace AshenVoid
{
	public class AshenVoid : Mod
	{
		internal enum MessageType : byte
		{
			SyncBossState
		}

		public override void HandlePacket(BinaryReader reader, int whoAmI)
		{
			MessageType msgType = (MessageType)reader.ReadByte();

			switch (msgType)
			{
				case MessageType.SyncBossState:
					if (Main.netMode == NetmodeID.MultiplayerClient)
					{
						byte npcWhoAmI = reader.ReadByte();
						byte stateId = reader.ReadByte();

						NPC npc = Main.npc[npcWhoAmI];
						if (npc.active && npc.ModNPC is EcsBoss boss)
						{
							var controller = boss.Controller;
							if (controller != null && controller.HasComponent<AIStateComponent>())
							{
								var aiState = controller.GetComponent<AIStateComponent>();
								var stateType = aiState.GetStateType(stateId);
								if (stateType != null)
								{
									var stateFactory = aiState.Blackboard.Get<StateFactory>(Core.ECS.AI.BlackboardKeys.StateFactory);
									var newState = stateFactory.GetState(stateType);
									// We call ChangeState directly on the client. 
									// The client-side ChangeState will not attempt to send another packet.
									aiState.StateMachine.ChangeState(newState, aiState.Blackboard);
								}
							}
						}
					}
					break;
			}
		}
	}
}