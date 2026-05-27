using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Delly.Terminal.Exception
{
    /// <summary>
    /// 命令异常
    /// </summary>
    public class CommandException : System.Exception
    {
        /// <summary>
        /// 命令异常
        /// </summary>
        /// <param name="message"></param>
        public CommandException(string message) : base(message) { }

        /// <summary>
        /// 命令异常
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innnerException"></param>
        public CommandException(string message, System.Exception innnerException) : base(message, innnerException) { }
    }
}


