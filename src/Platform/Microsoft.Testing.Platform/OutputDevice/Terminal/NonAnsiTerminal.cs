﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Testing.Platform.Helpers;
using Microsoft.Testing.Platform.Resources;

namespace Microsoft.Testing.Platform.OutputDevice.Terminal;

/// <summary>
/// Non-ANSI terminal that writes text using the standard Console.Foreground color capabilities to stay compatible with
/// standard Windows command line, and other command lines that are not capable of ANSI, or when output is redirected.
/// </summary>
internal sealed class NonAnsiTerminal : ITerminal
{
    private readonly IConsole _console;
    private readonly ConsoleColor _defaultForegroundColor;
    private bool _isBatching;

    public NonAnsiTerminal(IConsole console)
    {
        _console = console;
        _defaultForegroundColor = IsForegroundColorNotSupported() ? ConsoleColor.Black : _console.GetForegroundColor();
    }

#pragma warning disable CA1416 // Validate platform compatibility
    public int Width => _console.IsOutputRedirected ? int.MaxValue : _console.BufferWidth;

    public int Height => _console.IsOutputRedirected ? int.MaxValue : _console.BufferHeight;
#pragma warning restore CA1416 // Validate platform compatibility

    public void Append(char value)
        => _console.Write(value);

    public void Append(string value)
        => _console.Write(value);

    public void AppendLine()
        => _console.WriteLine();

    public void AppendLine(string value)
        => _console.WriteLine(value);

    public void AppendLink(string path, int? lineNumber)
    {
        Append(path);
        if (lineNumber.HasValue)
        {
            Append($":{lineNumber}");
        }
    }

    public void SetColor(TerminalColor color)
    {
        if (IsForegroundColorNotSupported())
        {
            return;
        }

        _console.SetForegroundColor(ToConsoleColor(color));
    }

    public void ResetColor()
    {
        if (IsForegroundColorNotSupported())
        {
            return;
        }

        _console.SetForegroundColor(_defaultForegroundColor);
    }

    public void ShowCursor()
    {
        // nop
    }

    public void HideCursor()
    {
        // nop
    }

    public void WithBatching(Action<ITerminal> action)
    {
        StartUpdate();

        try
        {
            action(this);
        }
        finally
        {
            StopUpdate();
        }
    }

    // TODO: Refactor NonAnsiTerminal and AnsiTerminal such that we don't need StartUpdate/StopUpdate.
    // It's much better if we use lock C# keyword instead of manually calling Monitor.Enter/Exit
    // Using lock also ensures we don't accidentally have `await`s in between that could cause Exit to be on a different thread.
    private void StartUpdate()
    {
        if (_isBatching)
        {
            throw new InvalidOperationException(PlatformResources.ConsoleIsAlreadyInBatchingMode);
        }

        bool lockTaken = false;
        // SystemConsole.ConsoleOut is set only once in static ctor.
        // So we are sure we will be doing Monitor.Exit on the same instance.
        // Note that we need to lock on System.Out for batching to work correctly.
        // Consider the following scenario:
        // 1. We call StartUpdate
        // 2. We call a Write("A")
        // 3. User calls Console.Write("B") from another thread.
        // 4. We call a Write("C").
        // 5. We call StopUpdate.
        // The expectation is that we see either ACB, or BAC, but not ABC.
        // Basically, when doing batching, we want to ensure that everything we write is
        // written continuously, without anything in-between.
        // One option (and we used to do it), is that we append to a StringBuilder while batching
        // Then at StopUpdate, we write the whole string at once.
        // This works to some extent, but we cannot get it to work when SetColor kicks in.
        // Console methods will internally lock on Console.Out, so we are locking on the same thing.
        // This locking is the easiest way to get coloring to work correctly while preventing
        // interleaving with user's calls to Console.Write methods.
        Monitor.Enter(SystemConsole.ConsoleOut, ref lockTaken);
        if (!lockTaken)
        {
            // Can this happen? :/
            throw new InvalidOperationException();
        }

        _isBatching = true;
    }

    private void StopUpdate()
    {
        Monitor.Exit(SystemConsole.ConsoleOut);
        _isBatching = false;
    }

    private ConsoleColor ToConsoleColor(TerminalColor color) => color switch
    {
        TerminalColor.Black => ConsoleColor.Black,
        TerminalColor.DarkRed => ConsoleColor.DarkRed,
        TerminalColor.DarkGreen => ConsoleColor.DarkGreen,
        TerminalColor.DarkYellow => ConsoleColor.DarkYellow,
        TerminalColor.DarkBlue => ConsoleColor.DarkBlue,
        TerminalColor.DarkMagenta => ConsoleColor.DarkMagenta,
        TerminalColor.DarkCyan => ConsoleColor.DarkCyan,
        TerminalColor.Gray => ConsoleColor.White,
        TerminalColor.Default => _defaultForegroundColor,
        TerminalColor.DarkGray => ConsoleColor.Gray,
        TerminalColor.Red => ConsoleColor.Red,
        TerminalColor.Green => ConsoleColor.Green,
        TerminalColor.Yellow => ConsoleColor.Yellow,
        TerminalColor.Blue => ConsoleColor.Blue,
        TerminalColor.Magenta => ConsoleColor.Magenta,
        TerminalColor.Cyan => ConsoleColor.Cyan,
        TerminalColor.White => ConsoleColor.White,
        _ => _defaultForegroundColor,
    };

    public void EraseProgress()
    {
        // nop
    }

    public void RenderProgress(TestProgressState?[] progress)
    {
        int count = 0;
        foreach (TestProgressState? p in progress)
        {
            if (p == null)
            {
                continue;
            }

            count++;

            string durationString = HumanReadableDurationFormatter.Render(p.Stopwatch.Elapsed);

            int passed = p.PassedTests;
            int failed = p.FailedTests;
            int skipped = p.SkippedTests;

            // Use just ascii here, so we don't put too many restrictions on fonts needing to
            // properly show unicode, or logs being saved in particular encoding.
            Append('[');
            SetColor(TerminalColor.DarkGreen);
            Append('+');
            Append(passed.ToString(CultureInfo.CurrentCulture));
            ResetColor();

            Append('/');

            SetColor(TerminalColor.DarkRed);
            Append('x');
            Append(failed.ToString(CultureInfo.CurrentCulture));
            ResetColor();

            Append('/');

            SetColor(TerminalColor.DarkYellow);
            Append('?');
            Append(skipped.ToString(CultureInfo.CurrentCulture));
            ResetColor();
            Append(']');

            Append(' ');
            Append(p.AssemblyName);

            if (p.TargetFramework != null || p.Architecture != null)
            {
                Append(" (");
                if (p.TargetFramework != null)
                {
                    Append(p.TargetFramework);
                    Append('|');
                }

                if (p.Architecture != null)
                {
                    Append(p.Architecture);
                }

                Append(')');
            }

            TestDetailState? activeTest = p.TestNodeResultsState?.GetRunningTasks(1).FirstOrDefault();
            if (!RoslynString.IsNullOrWhiteSpace(activeTest?.Text))
            {
                Append(" - ");
                Append(activeTest.Text);
                Append(' ');
            }

            Append(durationString);

            AppendLine();
        }

        // Do not render empty lines when there is nothing to show.
        if (count > 0)
        {
            AppendLine();
        }
    }

    public void StartBusyIndicator()
    {
        // nop
    }

    public void StopBusyIndicator()
    {
        // nop
    }

    [SupportedOSPlatformGuard("android")]
    [SupportedOSPlatformGuard("ios")]
    [SupportedOSPlatformGuard("tvos")]
    [SupportedOSPlatformGuard("browser")]
    private static bool IsForegroundColorNotSupported()
        => RuntimeInformation.IsOSPlatform(OSPlatform.Create("ANDROID")) ||
            RuntimeInformation.IsOSPlatform(OSPlatform.Create("IOS")) ||
            RuntimeInformation.IsOSPlatform(OSPlatform.Create("TVOS")) ||
            RuntimeInformation.IsOSPlatform(OSPlatform.Create("BROWSER"));
}
