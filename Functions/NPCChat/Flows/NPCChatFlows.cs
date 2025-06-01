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
        public string Text
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

        public NPCChatOption(string text, NPCChatParagraph next)
        {
            Text = text;
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
            get => _text;
        }

        /// <summary>
        /// 选项列表（可为空）
        /// </summary>
        public List<NPCChatOption>? Options
        {
            get; set;
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


        private string _text;

        public NPCChatParagraph(string text)
        {
            Next = this;
            _text = text;
        }
    }

    public class NPCChatParagraphFunc : NPCChatParagraph
    {
        /// <summary>
        /// 段落执行的动作
        /// </summary>
        public Func<string> GetText { get; set; }
        public override string Text
        {
            get => GetText() ?? base.Text;
        }
        public NPCChatParagraphFunc(Func<string> func) : base("")
        {
            GetText = func;
        }
    }

}
