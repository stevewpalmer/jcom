// JEdit
// Screen management
//
// Authors:
//  Steve Palmer
//
// Copyright (C) 2023 Steve Palmer
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

namespace JEdit;

public class Screen {
    private readonly List<Window> _windowList = new();
    private Window _activeWindow;
    private readonly StatusBar _statusBar = new();

    /// <summary>
    /// Add a window to the window list. This will not make the window
    /// active.
    /// </summary>
    /// <param name="theWindow"></param>
    public void AddWindow(Window theWindow) {
        _windowList.Add(theWindow);
    }
    
    /// <summary>
    /// Open the main window, rendering the frame, status bar and
    /// the active window.
    /// </summary>
    public void Open() {
        RenderFrame();
        _statusBar.InitialRender();
        ActivateWindow(0);
    }

    /// <summary>
    /// Render the title at the top of the screen.
    /// </summary>
    public static void RenderTitle(string title) {
        Console.SetCursorPosition(0, 0);
        Console.Write('╒');
        Console.Write(new string('═', Console.WindowWidth - 2));
        Console.Write('╕');
        Console.SetCursorPosition((Console.WindowWidth - title.Length - 2) / 2, 0);
        Console.Write($" {title} ");
    }

    /// <summary>
    /// Close the main screen when the editor is closed.
    /// </summary>
    public static void Close() {
        Console.Clear();
    }

    /// <summary>
    /// Start the editor keyboard loop and exit when the user issues the
    /// exit command.
    /// </summary>
    public void StartKeyboardLoop() {
        RenderHint flags;
        do {
            ConsoleKeyInfo keyIn = Console.ReadKey(true);
            _statusBar.Message($"Modifiers={keyIn.Modifiers}, Key={keyIn.Key}, KeyChar={(int)keyIn.KeyChar}");
            KeyCommand commandId = KeyMap.MapKeyToCommand(keyIn);
            flags = Handle(commandId);
        } while (flags != RenderHint.EXIT);
    }

    /// <summary>
    /// Handle keystrokes at the screen level and pass on any unhandled
    /// ones to the active window.
    /// </summary>
    /// <param name="commandId">Command ID</param>
    /// <returns>The rendering hint</returns>
    private RenderHint Handle(KeyCommand commandId) {
        RenderHint flags = RenderHint.NONE;
        switch (commandId) {
            case KeyCommand.KC_EXIT:
                flags = ExitEditor();
                break;
            
            case KeyCommand.KC_VERSION:
                _statusBar.RenderVersion();
                break;

            case KeyCommand.KC_NEXTBUFFER:
                flags = SelectWindow(1);                
                break;

            case KeyCommand.KC_PREVBUFFER:
                flags = SelectWindow(-1);                
                break;
            
            case KeyCommand.KC_CLOSE:
                flags = CloseWindow();
                break;

            case KeyCommand.KC_DETAILS:
                flags = ShowDetails();
                break;

            default:
                flags = _activeWindow.Handle(commandId);
                break;
        }
        if (flags.HasFlag(RenderHint.CURSOR_STATUS)) {
            UpdateCursorPosition();
        }
        return flags;
    }

    /// <summary>
    /// Render the current cursor position on the status bar.
    /// </summary>
    private void UpdateCursorPosition() {
        _statusBar.UpdateCursorPosition(_activeWindow.Buffer.LineIndex + 1, _activeWindow.Buffer.Offset + 1);
    }

    /// <summary>
    /// Select the next window in the specified direction in the window list.
    /// </summary>
    /// <param name="direction">Direction</param>
    /// <returns>Render hint</returns>
    private RenderHint SelectWindow(int direction) {
        if (_windowList.Count == 1) {
            _statusBar.Message("No other buffers.");
            return RenderHint.NONE;
        }
        int currentBufferIndex = _windowList.IndexOf(_activeWindow) + direction;
        if (currentBufferIndex == _windowList.Count) {
            currentBufferIndex = 0;
        }
        if (currentBufferIndex < 0) {
            currentBufferIndex = _windowList.Count - 1;
        }
        ActivateWindow(currentBufferIndex);
        return RenderHint.CURSOR_STATUS;
    }

    /// <summary>
    /// Activate a window by its index
    /// </summary>
    /// <param name="index">Index of the window to be activated</param>
    private void ActivateWindow(int index) {
        _activeWindow = _windowList[index];
        _activeWindow.SetViewportBounds(1, 1, Console.WindowWidth - 2, Console.WindowHeight - 3);
        _activeWindow.SetActive();
        UpdateCursorPosition();
    }

    /// <summary>
    /// Close the current window. You cannot close the window if this is
    /// the last window in the list.
    /// </summary>
    private RenderHint CloseWindow() {
        if (_windowList.Count == 1) {
            return RenderHint.NONE;
        }
        if (_activeWindow.Buffer.Modified) {
            do {
                ConsoleKey key = _statusBar.Prompt("This buffer has not been saved. Delete [ynw]?");
                if (key == ConsoleKey.N || key == ConsoleKey.Escape || key == ConsoleKey.Enter) {
                    return RenderHint.NONE;
                }
                if (key == ConsoleKey.Y) {
                    break;
                }
                if (key == ConsoleKey.W) {
                    _activeWindow.Buffer.Write();
                    break;
                }
            } while (true);
        }
        Window currentWindow = _activeWindow;
        RenderHint flags = SelectWindow(1);
        _windowList.Remove(currentWindow);
        return flags;
    }

    /// <summary>
    /// Show details of the file in the buffer on the status bar.
    /// </summary>
    private RenderHint ShowDetails() {
        _statusBar.Message(_activeWindow.Buffer.FullFilename + (_activeWindow.Buffer.Modified ? "*" : ""));
        return RenderHint.NONE;
    }

    /// <summary>
    /// Exit the editor, saving any buffers if required.
    /// </summary>
    /// <returns></returns>
    private RenderHint ExitEditor() {
        int modifiedBuffers = _windowList.Count(w => w.Buffer.Modified);
        if (modifiedBuffers > 0) {
            do {
                ConsoleKey key = _statusBar.Prompt($"{modifiedBuffers} buffers have not been saved. Exit [ynw]?");
                if (key == ConsoleKey.N) {
                    return RenderHint.NONE;
                }
                if (key == ConsoleKey.Y) {
                    break;
                }
                if (key == ConsoleKey.W) {
                    foreach (Window window in _windowList) {
                        window.Buffer.Write();
                    }
                    break;
                }
            } while(true);
        }
        return RenderHint.EXIT;
    }

    /// <summary>
    /// Draw the editor window frame
    /// </summary>
    private static void RenderFrame() {
        Console.Clear();

        Console.SetCursorPosition(0, 0);
        Console.Write('╒');
        Console.Write(new string('═', Console.WindowWidth - 2));
        Console.Write('╕');

        for (int c = 1; c < Console.WindowHeight - 2; c++) {
            Console.SetCursorPosition(0, c);
            Console.Write('│');
            Console.SetCursorPosition(Console.WindowWidth - 1, c);
            Console.Write('│');
        }

        Console.SetCursorPosition(0, Console.WindowHeight - 2);
        Console.Write('╘');
        Console.Write(new string('═', Console.WindowWidth - 2));
        Console.Write('╛');
    }

}