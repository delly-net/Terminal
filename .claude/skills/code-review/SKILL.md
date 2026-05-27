---
name: code-review
description: 代码审查技能 - Delly.Terminal 项目专用
---

# Delly.Terminal 代码审查

此技能用于 Delly.Terminal 项目的代码审查。

## 项目背景

Delly.Terminal 是一个 C# 类库，提供灵活的终端组件，支持实时输出监控和进程树管理。

## 构建命令

```bash
dotnet build Delly.Terminal\Delly.Terminal.csproj
```

## 架构要点

### 核心组件

- **Runner.cs** - 主终端运行器类
  - 实现 IDisposable 用于资源管理
  - 支持同步 (`Run()`) 和异步 (`Start()`) 命令执行
  - 通过 `OutputLine` 和 `ErrorLine` 事件提供实时输出
  - 仅在 Windows 上支持进程树管理（通过 `wmic`）
  - 处理 stdin/stdout/stderr 的字符编码

- **Event/CommandOutputLineEventArgs.cs** - 输出事件参数

- **Exception/CommandException.cs** - 命令执行异常

### 关键设计模式

- **事件驱动输出**: Runner 类暴露 `OutputLine` 和 `ErrorLine` 事件，每行输出时触发
- **字符级读取**: 使用 `StreamReader.Read()` 处理终端输出（包括制表符、控制字符和多字节 Unicode）
- **进程树管理**: 在 Windows 上维护父子进程映射，确保通过 `Kill()` 干净地终止进程

### .NET Standard 2.0 兼容性

库使用条件编译 (`#if NETSTANDARD2_0`) 保持与旧 .NET 版本的兼容性，同时在 .NET 6.0+ 上启用可空引用类型。

## 审查检查清单

- [ ] 确保 `#if NETSTANDARD2_0` 条件编译正确使用，保持多框架兼容性
- [ ] 检查 IDisposable 实现是否正确释放资源
- [ ] 验证线程安全（如 `_parentIds` 字典使用 lock）
- [ ] 检查事件处理避免内存泄漏
- [ ] 验证 Windows 特定代码有平台检查（`RuntimeInformation.IsOSPlatform(OSPlatform.Windows)`）
- [ ] 确保字符串编码处理正确（UTF-8 支持）
- [ ] 检查异常处理适当且有意义