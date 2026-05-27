using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Delly.Terminal.Event
{
    /// <summary>
    /// 输出行事件参数
    /// </summary>
    public class CommandOutputLineEventArgs : EventArgs
    {
        /// <summary>
        /// 内容
        /// </summary>
        public string Content { get; set; } = "";
    }
}


