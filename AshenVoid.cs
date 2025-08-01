using System.IO;
using Terraria.ModLoader;

namespace AshenVoid
{
	public class AshenVoid : Mod
	{
		public override void Load()
		{
			Core.Configuration.ConfigLoader.Load();
		}

		public override void Unload()
		{
			Core.Configuration.ConfigLoader.Unload();
		}
		// The old MessageType enum is no longer needed here, 
		// as the new NetworkManager handles message types internally.

		public override void HandlePacket(BinaryReader reader, int whoAmI)
		{
			// All packet handling is now delegated to the central NetworkManager.
			Core.ECS.EcsSystem.Instance.NetworkManager.HandlePacket(reader, whoAmI);
		}
	}
}