#if !NETSTANDARD2_0
#nullable enable
#endif

using Delly.Terminal.Event;
using Delly.Terminal.Exception;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Delly.Terminal
{
    /// <summary>
    /// 运行器
    /// </summary>
    public class Runner : IDisposable
    {
        private static readonly Dictionary<int, int> _parentIds;

        /// <summary>
        /// 常规字符
        /// </summary>
        public const string NORMAL_CHARS = "~!@#$%^&*()_+-=QWERTYUIOPASDFGHJKLZXCVBNMqwertyuiopasdfghjklzxcvbnm{}:|\"<>?[]\\;',./1234567890";
        /// <summary>
        /// 换行符
        /// </summary>
        public const int CHAR_ENTER = (byte)'\n';

        /// <summary>
        /// 命令行输出委托
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public delegate void CommandOutputLineEventHandler(object sender, CommandOutputLineEventArgs e);
        /// <summary>
        /// 命令行输出事件
        /// </summary>
        public event CommandOutputLineEventHandler OutputLine;
        /// <summary>
        /// 命令行异常输出事件
        /// </summary>
        public event CommandOutputLineEventHandler ErrorLine;

        // 私有变量
        private readonly ProcessStartInfo _processStartInfo;
        private readonly List<string> _outputs = new List<string>();
#if NETSTANDARD2_0
        private StreamWriter _input = null;
        private StreamReader _output = null;
        private StreamReader _error = null;
        private Process _process = null;
#else
        private StreamWriter? _input = null;
        private StreamReader? _output = null;
        private StreamReader? _error = null;
        private Process? _process = null;
#endif

        // 运行器
        static Runner()
        {
            _parentIds = new Dictionary<int, int>();
            // 自动加载进程信息
            Task.Run(() =>
            {
                UpdateProcessInfos();
            });
        }

        #region 构造函数

        /// <summary>
        /// 运行器
        /// </summary>
        public Runner(ProcessStartInfo processStartInfo)
        {
            OutputLine += new CommandOutputLineEventHandler((sender, e) => { });
            ErrorLine += new CommandOutputLineEventHandler((sender, e) => { });
            _outputs = new List<string>();
            Errors = new List<string>();
            _processStartInfo = processStartInfo;
            Init();
        }

        /// <summary>
        /// 运行器
        /// </summary>
        /// <param name="path">程序路径</param>
        /// <param name="arg">运行参数</param>
        /// <param name="workPath">工作目录</param>
        /// <param name="encodingName"></param>
#if NETSTANDARD2_0
        public Runner(string path, string arg, string workPath, string encodingName)
#else
        public Runner(string path, string? arg, string? workPath, string? encodingName)
#endif
        {
            OutputLine += new CommandOutputLineEventHandler((sender, e) => { });
            ErrorLine += new CommandOutputLineEventHandler((sender, e) => { });
            _outputs = new List<string>();
            Errors = new List<string>();
            _processStartInfo = new ProcessStartInfo(path);
            Init();
            if (!string.IsNullOrWhiteSpace(arg)) _processStartInfo.Arguments = arg;
            if (!string.IsNullOrWhiteSpace(workPath)) _processStartInfo.WorkingDirectory = workPath;
            if (!string.IsNullOrWhiteSpace(encodingName))
            {
                var encoding = Encoding.GetEncoding(encodingName);
#if !NETSTANDARD2_0
                _processStartInfo.StandardInputEncoding = Encoding.UTF8;
#endif
                _processStartInfo.StandardOutputEncoding = Encoding.UTF8;
                _processStartInfo.StandardErrorEncoding = Encoding.UTF8;
            }
        }

        /// <summary>
        /// 运行器
        /// </summary>
        /// <param name="path">程序路径</param>
        /// <param name="arg">运行参数</param>
        /// <param name="workPath">工作目录</param>
#if NETSTANDARD2_0
        public Runner(string path, string arg, string workPath) : this(path, arg, workPath, null) { }
#else
        public Runner(string path, string? arg, string? workPath) : this(path, arg, workPath, null) { }
#endif

        /// <summary>
        /// 运行器
        /// </summary>
        /// <param name="path">程序路径</param>
        /// <param name="arg">运行参数</param>
#if NETSTANDARD2_0
        public Runner(string path, string arg) : this(path, arg, null, null) { }
#else
        public Runner(string path, string? arg) : this(path, arg, null, null) { }
#endif

        /// <summary>
        /// 运行器
        /// </summary>
        /// <param name="path">程序路径</param>
        public Runner(string path) : this(path, null, null, null) { }

        #endregion

        /// <summary>
        /// 输入流
        /// </summary>
#if NETSTANDARD2_0
        public StreamWriter Input => _input;
#else
        public StreamWriter Input => _input!;
#endif

        // 更新进程信息集合
        private static void UpdateProcessInfos()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) { return; }
            // 获取所有父线程信息
            using (var runner = new Runner("wmic", $"process get parentprocessid,processid"))
            {
                var res = runner.Run();
                var lines = res.Replace("\r", "").Split('\n');
                // 没内容则退出
                if (lines.Length < 2) { return; }
                // 解析标题
                var heads = ParseStrings(lines[0]);
                int idIndex = heads.IndexOf("processid");
                int parentIdIndex = heads.IndexOf("parentprocessid");
                // 解析内容
                for (int i = 1; i < lines.Length; i++)
                {
                    var line = lines[i];
                    var values = ParseStrings(line);
                    if (values.Count != 2) { continue; }
                    var id = Convert.ToInt32(values[idIndex]);
                    var parentId = Convert.ToInt32(values[parentIdIndex]);
                    if (!_parentIds.ContainsKey(id))
                    {
                        lock (_parentIds) _parentIds.Add(id, parentId);
                    }
                }
            }
        }

        // 解析
        private static List<string> ParseStrings(string str)
        {
            var list = new List<string>();
            // 解析字符串
            var sb = new StringBuilder();
            for (var i = 0; i < str.Length; i++)
            {
                char chr = str[i];
                switch (chr)
                {
                    // 处理空格
                    case ' ':
                        if (sb.Length <= 0) { break; }
                        list.Add(sb.ToString().ToLower());
                        sb.Clear();
                        break;
                    // 添加内容
                    default:
                        sb.Append(chr);
                        break;
                }
            }
            if (sb.Length > 0)
            {
                list.Add(sb.ToString().ToLower());
                sb.Clear();
            }
            return list;
        }

        /// <summary>
        /// 输出内容
        /// </summary>
        public List<string> Outputs => _outputs;

        /// <summary>
        /// 错误内容
        /// </summary>
        public List<string> Errors { get; private set; }

        /// <summary>
        /// 是否运行中
        /// </summary>
        public bool IsRunning { get; private set; }

        /// <summary>
        /// 命令字符串
        /// </summary>
        public string Command { get; private set; } = "";

        // 初始化
        private void Init()
        {
            IsRunning = false;
            _processStartInfo.UseShellExecute = false;
            _processStartInfo.RedirectStandardOutput = true;
            _processStartInfo.RedirectStandardError = true;
            _processStartInfo.RedirectStandardInput = true;
            _processStartInfo.CreateNoWindow = true;
            if (_processStartInfo.Arguments == "")
            {
                Command = _processStartInfo.FileName;
            }
            else
            {
                Command = _processStartInfo.FileName + " " + _processStartInfo.Arguments;
            }
        }

        // 检测并创建新行
        private int CheckOrCreateLine(int line)
        {
            if (line < 0)
            {
                _outputs.Add("");
                line = _outputs.Count - 1;
            }
            return line;
        }

        // 获取输出内容
        private string GetOutputs()
        {
            var sb = new StringBuilder();
            for (int i = 0; i < _outputs.Count - 1; i++)
            {
                sb.AppendLine(_outputs[i]);
            }
            return sb.ToString();
        }

        // 读取
        private int Read(StreamReader reader, List<int> chars, StringBuilder sb, int line, bool isError)
        {
            int res = reader.Read();
            if (res > 255)
            {
                // 处理双字节
                sb.Append((char)res);
                line = CheckOrCreateLine(line);
                _outputs[line] = sb.ToString();
            }
            else if (chars.Contains(res))
            {
                // 处理常规字符
                sb.Append((char)res);
                line = CheckOrCreateLine(line);
                _outputs[line] = sb.ToString();
            }
            else
            {
                switch (res)
                {
                    case CHAR_ENTER:
                        // 触发事件
                        if (isError)
                        {
                            ErrorLine(this, new CommandOutputLineEventArgs() { Content = GetOutputs() });
                        }
                        else
                        {
                            OutputLine(this, new CommandOutputLineEventArgs() { Content = GetOutputs() });
                        }
                        // 回车
                        _outputs.Add("");
                        line = _outputs.Count - 1;
                        sb.Clear();
                        break;
                    case 0x09:
                        // Tab
                        line = CheckOrCreateLine(line);
                        var len = _outputs[line].Length;
                        sb.Append(new string(' ', 8 - len % 8));
                        _outputs[line] = sb.ToString();
                        break;
                    case '\r':
                    case ' ':
                        // 常规处理
                        sb.Append((char)res);
                        line = CheckOrCreateLine(line);
                        _outputs[line] = sb.ToString();
                        break;
                    default:
                        sb.Append($"[0x{res.ToString("x2")}]");
                        sb.Append((char)res);
                        line = CheckOrCreateLine(line);
                        _outputs[line] = sb.ToString();
                        break;
                }
            }
            return line;
        }

        /// <summary>
        /// 执行程序
        /// </summary>
        /// <returns></returns>
        public string Run()
        {
            var chars = new List<int>();
            for (int i = 0; i < NORMAL_CHARS.Length; i++) chars.Add(NORMAL_CHARS[i]);
            _process = Process.Start(_processStartInfo);
            if (_process is null) throw new CommandException($"Process '{_processStartInfo.FileName}' start fail.");
            IsRunning = true;
            _outputs.Clear();
            Errors.Clear();
            _process.Exited += Process_Exited;
            _input = _process.StandardInput;
            _output = _process.StandardOutput;
            _error = _process.StandardError;
            int outputLine = -1;
            int errorLine = -1;
            var sbOutput = new StringBuilder();
            var sbError = new StringBuilder();
            var thread = new Thread(() =>
            {
                try
                {
                    while (!_process.HasExited && !_output.EndOfStream)
                    {
                        outputLine = Read(_output, chars, sbOutput, outputLine, false);
                    }
                }
                catch { }
            });
            thread.Start();
            var thread2 = new Thread(() =>
            {
                try
                {
                    while (!_process.HasExited && !_error.EndOfStream)
                    {
                        errorLine = Read(_error, chars, sbError, errorLine, true);
                    }
                }
                catch { }
            });
            thread2.Start();
            _process.WaitForExit();
            try
            {
                while (!_output.EndOfStream)
                {
                    outputLine = Read(_output, chars, sbOutput, outputLine, false);
                }
            }
            catch { }
            try
            {
                while (!_error.EndOfStream)
                {
                    errorLine = Read(_error, chars, sbError, errorLine, true);
                }
            }
            catch { }
            var sb = new StringBuilder();
            foreach (var content in _outputs)
            {
                sb.AppendLine(content);
            }
            return sb.ToString();
        }

        /// <summary>
        /// 启动程序
        /// </summary>
        /// <returns></returns>
        public void Start()
        {
            var chars = new List<int>();
            for (int i = 0; i < NORMAL_CHARS.Length; i++) chars.Add(NORMAL_CHARS[i]);
            _process = Process.Start(_processStartInfo);
            if (_process is null) throw new CommandException($"程序'{_processStartInfo.FileName}'执行失败");
            IsRunning = true;
            _outputs.Clear();
            Errors.Clear();
            _process.EnableRaisingEvents = true;
            _process.Exited += Process_Exited;
            _input = _process.StandardInput;
            _output = _process.StandardOutput;
            _error = _process.StandardError;
            int outputLine = -1;
            int errorLine = -1;
            var sbOutput = new StringBuilder();
            var sbError = new StringBuilder();
            var thread = new Thread(() =>
            {
                try
                {
                    while (!_process.HasExited && !_output.EndOfStream)
                    {
                        outputLine = Read(_output, chars, sbOutput, outputLine, false);
                    }
                }
                catch { }
            });
            thread.Start();
            var thread2 = new Thread(() =>
            {
                try
                {
                    while (!_process.HasExited && !_error.EndOfStream)
                    {
                        errorLine = Read(_error, chars, sbError, errorLine, true);
                    }
                }
                catch { }
            });
            thread2.Start();
        }

        /// <summary>
        /// 关闭程序
        /// </summary>
        public void Close()
        {
            _process?.Close();
        }

        /// <summary>
        /// 杀死程序
        /// </summary>
        public void Kill()
        {
            if (_process is null) { return; }
            // 更新进程信息
            UpdateProcessInfos();
            var pids = GetChildProcessTree(_process.Id);
            var processes = Process.GetProcesses();
            KillByPids(pids, processes);
        }

        // 获取进程树
        private static List<int> GetChildProcessTree(int pid)
        {
            var pids = new List<int> { pid };
            pids.AddRange(GetChildProcesses(pid, true));
            return pids;
        }

        // 获取子进程
        private static List<int> GetChildProcesses(int pid, bool isDepth)
        {
            var pids = new List<int>();
            foreach (var pair in _parentIds)
            {
                if (pair.Value != pid) { continue; }
                pids.Add(pair.Key);
                if (!isDepth) { continue; }
                pids.AddRange(GetChildProcesses(pair.Key, true));
            }
            return pids;
        }

        // 杀死所有进程
        private static void KillByPids(List<int> pids, Process[] processes)
        {
            // 遍历所有的进程
            foreach (var process in processes)
            {
                if (!pids.Contains(process.Id)) { continue; }
                Debug.WriteLine($"[{process.Id}] {process.ProcessName}");
                try { process.Kill(); } catch { }
            }
        }

        // 线程退出
#if NETSTANDARD2_0
        private void Process_Exited(object sender, EventArgs e)
#else
        private void Process_Exited(object? sender, EventArgs e)
#endif
        {
            IsRunning = false;
        }

        /// <summary>
        /// 释放
        /// </summary>
        public void Dispose()
        {
            //throw new NotImplementedException();
            _input?.Dispose();
            _output?.Dispose();
            _error?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}

