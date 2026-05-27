# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Delly.Terminal is a C# class library that provides a flexible terminal component for developers. It allows running external processes with real-time output monitoring and process tree management.

## Build

```bash
dotnet build Delly.Terminal\Delly.Terminal.csproj
```

Target frameworks: netstandard2.0, net6.0

## Architecture

### Core Components

- **[Runner.cs](Delly.Terminal/Runner.cs)** - Main terminal runner class
  - Implements IDisposable for resource management
  - Supports both synchronous (`Run()`) and asynchronous (`Start()`) command execution
  - Provides real-time output events via `OutputLine` and `ErrorLine` events
  - Manages process tree for clean termination (Windows only, via `wmic`)
  - Handles character encoding for stdin/stdout/stderr

- **[Event/CommandOutputLineEventArgs.cs](Delly.Terminal/Event/CommandOutputLineEventArgs.cs)** - Event arguments for output line events

- **[Exception/CommandException.cs](Delly.Terminal/Exception/CommandException.cs)** - Custom exception for command execution failures

### Key Design Patterns

- **Event-driven output**: The Runner class exposes events (`OutputLine`, `ErrorLine`) that fire on each line of output, enabling real-time processing
- **Character-level reading**: Uses `StreamReader.Read()` with custom character handling for terminal output (including tabs, control characters, and multi-byte Unicode)
- **Process tree management**: On Windows, maintains a parent-child process mapping to ensure clean process termination via `Kill()`

### .NET Standard 2.0 Compatibility

The library uses conditional compilation (`#if NETSTANDARD2_0`) to maintain compatibility with older .NET versions while enabling nullable reference types on .NET 6.0+.