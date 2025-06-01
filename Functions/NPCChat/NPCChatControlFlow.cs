using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AshenVoid.Functions.NPCChat.Flows;

namespace AshenVoid.Functions.NPCChat
{
    /// <summary>
    /// NPC对话流程控制
    /// </summary>
    public class NPCChatControlFlow
    {
        private NPCChatParagraph? _current;
        private float _timer = 0f;
        private bool _waitingForOption = false;

        public NPCChatParagraph? Current => _current;

        public NPCChatControlFlow()
        {
        }

        /// <summary>
        /// 启动对话流程
        /// </summary>
        public void Start(NPCChatParagraph start)
        {
            _current = start;
            _timer = 0f;
            _waitingForOption = false;
        }

        /// <summary>
        /// 更新流程，deltaTime为秒
        /// </summary>
        public void Update(float deltaTime, NPCChatUI ui)
        {
            if (_current == null)
                return;
            if (_waitingForOption)
                return;
            if (!ui.IsReady)
                return;

            _timer += deltaTime;
            if (_timer >= _current.Interval)
            {
                if (_current.Options != null && _current.Options.Count > 0)
                {
                    // 等待玩家选择
                    _waitingForOption = true;
                }
                else
                {
                    // 自动跳转到下一个段落
                    GoToNext();
                }
            }
        }

        /// <summary>
        /// 玩家选择某个选项
        /// </summary>
        public void SelectOption(int index)
        {
            if (_current == null || _current.Options == null || index < 0 || index >= _current.Options.Count)
                return;

            _current = _current.Options[index].Next;
            _timer = 0f;
            _waitingForOption = false;
        }

        /// <summary>
        /// 跳转到下一个段落（支持随机）
        /// </summary>
        private void GoToNext()
        {
            if (_current == null)
                return;

            NPCChatParagraph oldParagraph = _current;
            if (_current.RandomNexts != null && _current.RandomNexts.Count > 0)
            {
                var rand = new Random();
                int idx = rand.Next(_current.RandomNexts.Count);
                _current = _current.RandomNexts[idx];
            }
            else
            {
                _current = _current.Next;
            }
            _timer = 0f;
            _waitingForOption = false;
        }

        /// <summary>
        /// 跳过当前段落，立即切换到下一个
        /// </summary>
        public void Skip()
        {
            GoToNext();
        }

        /// <summary>
        /// 是否等待玩家选择
        /// </summary>
        public bool WaitingForOption => _waitingForOption;
    }
}
