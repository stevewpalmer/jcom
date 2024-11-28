// JCom Runtime Library
// Readline class
// 
// Authors:
//  Steven Palmer
// 
// Copyright (C) 2021 Steven Palmer
// 
// Licensed to the Apache Software Foundation (ASF) under one
// or more contributor license agreements.  See the NOTICE file
// distributed with this work for additional information
// regarding copyright ownership.  The ASF licenses this file
// to you under the Apache License, Version 2.0 (the
// "License"); you may not use this file except in compliance
// with the License.  You may obtain a copy of the License at
// 
// # http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing,
// software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, either express or implied.  See the License for the
// specific language governing permissions and limitations
// under the License.

using System.Text;

namespace JComLib;

public class ReadLine {

    private static readonly List<string> history = [];

    private int cursorPosition = Console.CursorLeft;
    private StringBuilder buffer;
    private int bufferIndex;

    /// <summary>
    /// Line termination behaviour
    /// </summary>
    public LineTerminator Terminator { get; init; }

    /// <summary>
    /// Zone width for zone separated terminator
    /// </summary>
    public int Zone { get; init; }

    /// <summary>
    /// Maximum width of input (-1 means unconstrained)
    /// </summary>
    public int MaxWidth { get; init; }

    /// <summary>
    /// Whether to use the history stack.
    /// </summary>
    public bool AllowHistory { get; init; }

    /// <summary>
    /// Whether to allow filename completion
    /// </summary>
    public bool AllowFilenameCompletion { get; init; }

    /// <summary>
    /// Default constructor
    /// </summary>
    public ReadLine() {
        Terminator = LineTerminator.NEWLINE;
        Zone = 0;
        MaxWidth = -1;
        AllowHistory = false;
    }

    /// <summary>
    /// Line input function
    /// </summary>
    /// <param name="existingLine">Existing text to display</param>
    /// <returns>Input string</returns>
    public string Read(string existingLine) {

        if (Console.IsInputRedirected) {
            return Console.ReadLine();
        }

        int historyIndex = history.Count;
        string[] allfiles = null;
        int allfilesIndex = 0;
        string beforePart = string.Empty;
        string afterPart = string.Empty;

        SetBuffer(existingLine);

        ConsoleKeyInfo keyInfo = Console.ReadKey(true);
        while (keyInfo.Key != ConsoleKey.Enter) {
            switch (keyInfo.Key) {
                case ConsoleKey.Escape:
                    ClearInput();
                    break;

                case ConsoleKey.UpArrow:
                    if (historyIndex > 0 && AllowHistory) {
                        allfiles = null;
                        ClearInput();
                        SetBuffer(history[--historyIndex]);
                    }
                    break;

                case ConsoleKey.DownArrow:
                    if (historyIndex < history.Count - 1 && AllowHistory) {
                        allfiles = null;
                        ClearInput();
                        SetBuffer(history[++historyIndex]);
                    }
                    break;

                case ConsoleKey.LeftArrow:
                    if (bufferIndex > 0) {
                        Console.SetCursorPosition(--cursorPosition, Console.CursorTop);
                        bufferIndex--;
                    }
                    break;

                case ConsoleKey.RightArrow:
                    if (bufferIndex < buffer.Length) {
                        Console.SetCursorPosition(++cursorPosition, Console.CursorTop);
                        bufferIndex++;
                    }
                    break;

                case ConsoleKey.Delete:
                    if (bufferIndex < buffer.Length) {
                        allfiles = null;
                        buffer.Remove(bufferIndex, 1);
                        Console.Write(buffer.ToString()[bufferIndex..] + " ");
                        Console.SetCursorPosition(cursorPosition, Console.CursorTop);
                    }
                    break;

                case ConsoleKey.Backspace:
                    if (bufferIndex > 0) {
                        allfiles = null;
                        buffer.Remove(--bufferIndex, 1);
                        Console.SetCursorPosition(--cursorPosition, Console.CursorTop);
                        Console.Write(buffer.ToString()[bufferIndex..] + " ");
                        Console.SetCursorPosition(cursorPosition, Console.CursorTop);
                    }
                    break;

                case ConsoleKey.Tab:
                    if (AllowFilenameCompletion) {
                        if (allfiles == null) {
                            string partialName = buffer.ToString();
                            if (bufferIndex > 0) {
                                int startIndex = partialName.LastIndexOf(' ', bufferIndex - 1);
                                if (startIndex < 0) {
                                    startIndex = 0;
                                    beforePart = string.Empty;
                                }
                                else {
                                    beforePart = partialName[..(startIndex + 1)];
                                }
                                partialName = partialName[(startIndex + 1)..];
                                startIndex = partialName.IndexOf(' ');
                                if (startIndex < 0) {
                                    startIndex = partialName.Length;
                                    afterPart = string.Empty;
                                }
                                else {
                                    afterPart = partialName[startIndex..];
                                }
                                partialName = partialName[..startIndex];
                            }
                            allfiles = Directory.GetFiles(".", partialName + "*", SearchOption.TopDirectoryOnly);
                            allfilesIndex = 0;
                        }
                        if (allfiles.Length > 0) {
                            string completedName = new FileInfo(allfiles[allfilesIndex++]).Name;
                            if (allfilesIndex == allfiles.Length) {
                                allfilesIndex = 0;
                            }
                            ClearInput();
                            SetBuffer($"{beforePart}{completedName}{afterPart}");
                        }
                    }
                    break;

                case ConsoleKey.Home:
                    while (bufferIndex > 0) {
                        bufferIndex--;
                        cursorPosition--;
                    }
                    Console.SetCursorPosition(cursorPosition, Console.CursorTop);
                    break;

                case ConsoleKey.End:
                    while (bufferIndex < buffer.Length) {
                        bufferIndex++;
                        cursorPosition++;
                    }
                    Console.SetCursorPosition(cursorPosition, Console.CursorTop);
                    break;

                default:
                    if (!char.IsControl(keyInfo.KeyChar) && (MaxWidth == -1 || buffer.Length < MaxWidth)) {
                        allfiles = null;
                        buffer.Insert(bufferIndex, keyInfo.KeyChar);
                        Console.Write(buffer.ToString()[bufferIndex..]);
                        cursorPosition++;
                        bufferIndex++;
                        Console.SetCursorPosition(cursorPosition, Console.CursorTop);
                    }
                    break;
            }
            keyInfo = Console.ReadKey(true);
        }
        while (bufferIndex < buffer.Length) {
            bufferIndex++;
            cursorPosition++;
        }
        Console.SetCursorPosition(cursorPosition, Console.CursorTop);
        switch (Terminator) {
            case LineTerminator.NEWLINE:
                Console.WriteLine();
                break;

            case LineTerminator.NEXTZONE:
                if (Zone > 0) {
                    int numberOfSpaces = Zone - (cursorPosition + Zone) % Zone;
                    while (numberOfSpaces > 0) {
                        Console.Write(" ");
                        numberOfSpaces--;
                    }
                }
                break;

            case LineTerminator.NONE:
                Console.Write(" ");
                break;
        }
        if (AllowHistory) {
            history.Add(buffer.ToString());
        }
        return buffer.ToString();
    }

    // Clear the input and reset to the left edge.
    private void ClearInput() {
        int leftEdge = cursorPosition - bufferIndex;
        Console.SetCursorPosition(leftEdge, Console.CursorTop);
        Console.Write(new string(' ', buffer.Length));
        bufferIndex = 0;
        buffer.Clear();
        cursorPosition = leftEdge;
        Console.SetCursorPosition(cursorPosition, Console.CursorTop);
    }

    // Set the current input buffer from the specified string
    private void SetBuffer(string newString) {
        buffer = new StringBuilder(newString);
        cursorPosition = Console.CursorLeft;
        bufferIndex = 0;
        if (!string.IsNullOrEmpty(newString)) {
            Console.Write(newString);
            cursorPosition += newString.Length;
            bufferIndex = newString.Length;
            Console.SetCursorPosition(cursorPosition, Console.CursorTop);
        }
    }
}