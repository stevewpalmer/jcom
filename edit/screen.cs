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

    /// <summary>
    /// Constructor
    /// </summary>
    public Screen() {
        StatusBar = new StatusBar();
    }
        
    /// <summary>
    /// The status bar
    /// </summary>
    public static StatusBar StatusBar { get; private set; }

    /// <summary>
    /// Add a window to the window list. This will not make the window
    /// active.
    /// </summary>
    /// <param name="theWindow"></param>
    public void AddWindow(Window theWindow) {
        if (theWindow.Buffer.NewFile) {
            StatusBar.Message($"New file (unable to open file {theWindow.Buffer.BaseFilename})");
        }
        _windowList.Add(theWindow);
        theWindow.SetViewportBounds(1, 1, Console.WindowWidth - 2, Console.WindowHeight - 3);
    }
    
    /// <summary>
    /// Open the main window.
    /// </summary>
    public static void Open() {
        Console.Clear();
        StatusBar.InitialRender();
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
        RenderHint flags = commandId switch {
            KeyCommand.KC_CLOSE => CloseWindow(),
            KeyCommand.KC_COMMAND => RunCommand(),
            KeyCommand.KC_DETAILS => ShowDetails(),
            KeyCommand.KC_EDIT => EditFile(),
            KeyCommand.KC_EXIT => ExitEditor(true),
            KeyCommand.KC_NEXTBUFFER => SelectWindow(1),
            KeyCommand.KC_OUTPUTFILE => RenameOutputFile(),
            KeyCommand.KC_PREVBUFFER => SelectWindow(-1),
            KeyCommand.KC_REPEAT => Repeat(),
            KeyCommand.KC_VERSION => Version(),
            KeyCommand.KC_WRITEANDEXIT => ExitEditor(false),
            _ => _activeWindow.Handle(commandId)
        };
        if (flags.HasFlag(RenderHint.CURSOR_STATUS)) {
            UpdateCursorPosition();
        }
        return flags;
    }

    /// <summary>
    /// Render the current cursor position on the status bar.
    /// </summary>
    private void UpdateCursorPosition() {
        StatusBar.UpdateCursorPosition(_activeWindow.Buffer.LineIndex + 1, _activeWindow.Buffer.Offset + 1);
    }

    /// <summary>
    /// Select the next window in the specified direction in the window list.
    /// </summary>
    /// <param name="direction">Direction</param>
    private RenderHint SelectWindow(int direction) {
        if (_windowList.Count == 1) {
            StatusBar.Message("No other buffers.");
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
    public void ActivateWindow(int index) {
        _activeWindow = _windowList[index];
        _activeWindow.SetActive();
        UpdateCursorPosition();
    }

    /// <summary>
    /// Edit a file in a new window, or switch to the file in an existing window.
    /// </summary>
    private RenderHint EditFile() {
        if (StatusBar.PromptForInput("File:", out string inputValue, true)) {
            FileInfo fileInfo = new FileInfo(inputValue);
            inputValue = fileInfo.FullName;

            Window newWindow = _windowList.FirstOrDefault(window => window.Buffer.FullFilename == inputValue);
            if (newWindow == null) {
                newWindow = new Window(new Buffer(inputValue));
                AddWindow(newWindow);
            }
            _activeWindow = newWindow;
            _activeWindow.SetViewportBounds(1, 1, Console.WindowWidth - 2, Console.WindowHeight - 3);
            _activeWindow.SetActive();
            UpdateCursorPosition();
        }
        return RenderHint.NONE;
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
            char[] validInput = { 'y', 'n', 'w' }; 
            if (StatusBar.Prompt("This buffer has not been saved. Delete @@?", validInput, 'n', out char inputChar)) {
                switch (inputChar) {
                    case 'n':
                        return RenderHint.NONE;
                    case 'w':
                        _activeWindow.Buffer.Write();
                        break;
                }
            }
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
        StatusBar.Message(_activeWindow.Buffer.FullFilename + (_activeWindow.Buffer.Modified ? "*" : ""));
        return RenderHint.NONE;
    }

    /// <summary>
    /// Run a user specified command with optional parameters
    /// </summary>
    private RenderHint RunCommand() {
        RenderHint flags = RenderHint.NONE;
        if (StatusBar.PromptForInput("Command:", out string inputValue, false)) {
            KeyCommand commandId = KeyMap.MapCommandNameToCommand(inputValue);
            if (commandId == KeyCommand.KC_NONE) {
                StatusBar.Message("Unknown command");
            }
            else {
                flags = Handle(commandId);
            }
        }
        return flags;
    }

    /// <summary>
    /// Show the editor version on the status bar.
    /// </summary>
    private static RenderHint Version() {
        StatusBar.RenderVersion();
        return RenderHint.NONE;
    }

    /// <summary>
    /// Repeat a key command.
    /// </summary>
    private RenderHint Repeat() {
        RenderHint flags = RenderHint.NONE;
        if (StatusBar.PromptForRepeat(out int repeatCount, out KeyCommand commandId)) {
            while (repeatCount-- > 0) {
                flags |= Handle(commandId);
            }
        }
        return flags;
    }

    /// <summary>
    /// Prompt for a new output file name for the current buffer. The new
    /// name must not conflict with any existing buffer name otherwise an error
    /// is displayed.
    /// </summary>
    private RenderHint RenameOutputFile() {
        if (StatusBar.PromptForInput("Enter new output file name:", out string outputFileName, false)) {
            string fullFilename = Buffer.GetFullFilename(outputFileName);
            if (_windowList.Any(window => fullFilename.Equals(window.Buffer.FullFilename, StringComparison.OrdinalIgnoreCase))) {
                StatusBar.Message("Invalid output filename");
                return RenderHint.NONE;
            }
            _activeWindow.Buffer.Filename = outputFileName;
            foreach (Window window in _windowList.Where(window => window.Buffer == _activeWindow.Buffer)) {
                window.ApplyRenderHint(RenderHint.TITLE);
            }
        }
        return RenderHint.NONE;
    }

    /// <summary>
    /// Exit the editor, saving any buffers if required. If prompt is
    /// TRUE, we prompt whether to save or exit without saving. If prompt
    /// is FALSE, we just save all modified buffers and exit.
    /// </summary>
    private RenderHint ExitEditor(bool prompt) {
        RenderHint flags = RenderHint.EXIT;
        bool writeBuffers = !prompt;
        Buffer [] modifiedBuffers = _windowList.Where(w => w.Buffer.Modified).Select(b => b.Buffer).ToArray();
        if (prompt) {
            if (modifiedBuffers.Any()) {
                char[] validInput = { 'y', 'n', 'w' };
                if (StatusBar.Prompt($"{modifiedBuffers} buffers have not been saved. Exit @@?", validInput, 'n', out char inputChar)) {
                    switch (inputChar) {
                        case 'n':
                            flags = RenderHint.NONE;
                            break;
                        case 'w':
                            writeBuffers = true;
                            break;
                    }
                }
            }
        }
        if (writeBuffers) {
            foreach (Buffer buffer in modifiedBuffers) {
                buffer.Write();
            }
        }
        return flags;
    }
}