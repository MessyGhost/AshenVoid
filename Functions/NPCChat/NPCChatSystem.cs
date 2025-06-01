using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using AshenVoid.Content.女主;
using AshenVoid.Functions.NPCChat.Flows;

namespace AshenVoid.Functions.NPCChat
{
    internal class NPCChatSystem : ModSystem
    {
        private NPCChatUI _activeNPCChatUI;
        private bool _isInNPCChat;
        private NPCChatParagraph _oldChatParagraph;


        public override void Load()
        {
            On_Main.GUIChatDraw += On_Main_GUIChatDraw;
        }

        public override void PostSetupContent()
        {
            _activeNPCChatUI = new NPCChatUI();
            _oldChatParagraph = null;
        }

        private bool TryReplaceNPCChatGUI()
        {
            // 如果处于对话状态
            if ((Main.npcChatText != "" || Main.LocalPlayer.sign != -1) && !Main.editChest)
            {
                int npcID = Main.LocalPlayer.talkNPC;
                if (npcID != -1 && Main.npc[npcID].type == ModContent.NPCType<ExamplePerson>())
                {
                    _activeNPCChatUI.Activate(Main.npc[npcID]);
                    _activeNPCChatUI.Update();
                    _activeNPCChatUI.Draw(Main.spriteBatch);
                    return true;
                }
            }
            return false;
        }

        public override void PostUpdateNPCs()
        {
            if ((Main.npcChatText != "" || Main.LocalPlayer.sign != -1) && !Main.editChest)
            {
                int npcID = Main.LocalPlayer.talkNPC;
                if (npcID != -1 && Main.npc[npcID].type == ModContent.NPCType<ExamplePerson>())
                {
                    NPC npc = Main.npc[npcID];
                    if (!_isInNPCChat)
                    {
                        _isInNPCChat = true;
                        OnEnterNPCChat(npc);
                    }
                    var examplePerson = (ExamplePerson)npc.ModNPC;
                    // 将NPC的对话流程数据上传给UI渲染模块
                    var flow = examplePerson.GetFlow;
                    flow.Update(1 / 60.0f, _activeNPCChatUI);
                    int option = _activeNPCChatUI.GetAndClearChosenOption();
                    if (option != -1)
                    {
                        flow.SelectOption(option);
                    }
                    if (_oldChatParagraph != flow.Current)
                    {
                        _oldChatParagraph = flow.Current;
                        OnNPCParagraphChanged(flow);
                    }
                }
            }
            else
            {
                if (_isInNPCChat)
                {
                    _isInNPCChat = false;
                }
            }
        }

        private void OnEnterNPCChat(NPC npc)
        {
            var examplePerson = (ExamplePerson)npc.ModNPC;
            // 刷新UI状态
            var flow = examplePerson.GetFlow;
            OnNPCParagraphChanged(flow);
        }

        private void OnNPCParagraphChanged(NPCChatControlFlow flow)
        {
            if (flow.Current != null)
            {
                _activeNPCChatUI.Text = flow.Current.Text;

                if (flow.Current.Options != null && flow.Current.Options.Count > 0)
                {
                    _activeNPCChatUI.SetOptions(flow.Current.Options.Select(o => o.Text).ToList());

                }
                else
                {
                    _activeNPCChatUI.SetOptions(new List<string>());
                }
            }
        }
        private void On_Main_GUIChatDraw(On_Main.orig_GUIChatDraw orig, Main self)
        {
            if (!TryReplaceNPCChatGUI())
            {
                _activeNPCChatUI.Deactivate();
                orig(self);
            }
        }
    }
}
