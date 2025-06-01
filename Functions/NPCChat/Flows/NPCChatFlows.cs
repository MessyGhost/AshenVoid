using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AshenVoid.Functions.NPCChat.Flows
{
    /// <summary>
    /// 对话选项
    /// </summary>
    public class NPCChatOption
    {
        /// <summary>
        /// 选项文本
        /// </summary>
        public Func<string> GetText
        {
            get; set;
        }

        /// <summary>
        /// 选择该选项后跳转的段落
        /// </summary>
        public NPCChatParagraph Next
        {
            get; set;
        }

        public NPCChatOption(Func<string> text, NPCChatParagraph next)
        {
            GetText = text;
            Next = next;
        }
    }

    /// <summary>
    /// 对话段落
    /// </summary>
    public class NPCChatParagraph
    {
        /// <summary>
        /// 段落文本
        /// </summary>
        public virtual string Text
        {
            get => _text.Invoke();
        }

        /// <summary>
        /// 选项列表（可为空）
        /// </summary>
        public virtual List<NPCChatOption>? Options
        {
            get;
        }

        /// <summary>
        /// 下一个段落（可为null，表示结束）
        /// </summary>
        public NPCChatParagraph? Next
        {
            get; set;
        }

        /// <summary>
        /// 随机段落集合（如果设置，Next无效，优先级高于Next）
        /// </summary>
        public List<NPCChatParagraph>? RandomNexts
        {
            get; set;
        }

        /// <summary>
        /// 段落切换间隔（秒）
        /// </summary>
        public float Interval { get; set; } = 0f;


        private Func<string> _text;

        public NPCChatParagraph(Func<string> text)
        {
            Next = this;
            _text = text;
        }

        /// <summary>
        /// 决定UI是否立即显示该段落文本，而不是打字机效果
        /// </summary>
        public virtual bool ImmediateShow
        {
            get => false;
        }

        /// <summary>
        /// 当用户选择了某个选项时调用
        /// </summary>
        /// <param name="index"></param>
        public virtual void UserChooseOption(int index)
        {
        }

        /// <summary>
        /// 当用户点击下一步时调用
        /// </summary>
        public virtual void UserNext()
        {

        }
    }

    public class NPCChatLoopBackAllOptionsParagraph : NPCChatParagraph
    {
        private List<(Func<string>, NPCChatParagraph)> _options;
        private int _initalOptionsCount;
        public NPCChatLoopBackAllOptionsParagraph(Func<string> func, List<(Func<string>, NPCChatParagraph)> options) : base(func)
        {
            _options = options;
            _initalOptionsCount = options.Count;
            foreach (var (option, para) in options)
            {
                para.Next = this; // 设置每个选项的下一段落为当前段落
            }
        }
        public override bool ImmediateShow => _options.Count < _initalOptionsCount;

        public override List<NPCChatOption>? Options
        {
            get
            {
                if (_options == null || _options.Count == 0)
                    return null;
                return _options.Select(option => new NPCChatOption(option.Item1, option.Item2)).ToList();
            }
        }

        public override void UserChooseOption(int index)
        {
            // 移除选择了的选项
            _options.RemoveAt(index);

            if (_options.Count == 1)
            {
                _options[0].Item2.Next = Next;
            }
        }
    }

}
