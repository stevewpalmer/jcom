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

using JComLib;
using JEdit.Resources;

namespace JEdit;

public class Screen {
    private readonly List<Window> _windowList = new();
    private Window _activeWindow;
    private readonly Recorder _recorder = new();
    private Search _search;

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
    /// Configured colours
    /// </summary>
    public static Colours Colours { get; private set; }

    /// <summary>
    /// Configuration
    /// </summary>
    public static Config Config { get; private set; }

    /// <summary>
    /// Scrap buffer
    /// </summary>
    public static Buffer ScrapBuffer { get; private set; }

    /// <summary>
    /// Open the main window.
    /// </summary>
    public static void Open() {
        Terminal.Open();

        Config = Config.Load();
        Colours = new Colours(Config);
        ScrapBuffer = new Buffer();

        StatusBar.ShowClock = Config.ShowClock;
        StatusBar.Refresh();
        Version();
    }

    /// <summary>
    /// Close the main screen when the editor is closed.
    /// </summary>
    public static void Close() {
        Config.Save();
        Terminal.Close();
    }

    /// <summary>
    /// Start the editor keyboard loop and exit when the user issues the
    /// exit command.
    /// </summary>
    public void StartKeyboardLoop() {
        RenderHint flags;
        do {
            ConsoleKeyInfo keyIn = Console.ReadKey(true);
            flags = HandleCommand(KeyMap.MapKeyToCommand(keyIn));
        } while (flags != RenderHint.EXIT);
    }

    /// <summary>
    /// Handle commands at the screen level and pass on any unhandled
    /// ones to the active window.
    /// </summary>
    /// <param name="command">Editing command</param>
    /// <returns>Render hint</returns>
    private RenderHint HandleCommand(Command command) {
        if (StatusBar.KeystrokesMode == KeystrokesMode.RECORDING && KeyMap.IsRecordable(command.Id)) {
            if (!_recorder.RememberKeystroke(command)) {
                StatusBar.Error(Edit.MaximumKeystrokes);
                StatusBar.KeystrokesMode = KeystrokesMode.NONE;
            }
        }
        _activeWindow.PreCommand();
        RenderHint flags = command.Id switch {
            KeyCommand.KC_ASSIGNTOKEY => AssignToKey(command),
            KeyCommand.KC_BACKUPFILE => ToggleBackup(),
            KeyCommand.KC_BORDERS => ToggleBorders(),
            KeyCommand.KC_CD => ChangeDirectory(command),
            KeyCommand.KC_CLOCK => ToggleClock(),
            KeyCommand.KC_CLOSE => CloseWindow(),
            KeyCommand.KC_COLOUR => ConfigureColours(command),
            KeyCommand.KC_COMMAND => RunCommand(),
            KeyCommand.KC_DELFILE => DeleteFile(command),
            KeyCommand.KC_DETAILS => ShowDetails(),
            KeyCommand.KC_EDIT => EditFile(command),
            KeyCommand.KC_EXIT => ExitEditor(true),
            KeyCommand.KC_LOADKEYSTROKES => LoadRecording(),
            KeyCommand.KC_NEXTBUFFER => SelectWindow(1),
            KeyCommand.KC_OUTPUTFILE => RenameOutputFile(command),
            KeyCommand.KC_PLAYBACK => Playback(),
            KeyCommand.KC_PREVBUFFER => SelectWindow(-1),
            KeyCommand.KC_REMEMBER => StartStopRecording(),
            KeyCommand.KC_REPEAT => Repeat(),
            KeyCommand.KC_SAVEKEYSTROKES => SaveRecording(),
            KeyCommand.KC_SEARCHAGAIN => SearchAgain(),
            KeyCommand.KC_SEARCHBACK => Search(command, false),
            KeyCommand.KC_SEARCHCASE => SearchCaseToggle(),
            KeyCommand.KC_SEARCHFORWARD => Search(command, true),
            KeyCommand.KC_TRANSLATE => Translate(command, true),
            KeyCommand.KC_TRANSLATEAGAIN => TranslateAgain(),
            KeyCommand.KC_TRANSLATEBACK => Translate(command, false),
            KeyCommand.KC_REGEXP => RegExpToggle(),
            KeyCommand.KC_VERSION => Version(),
            KeyCommand.KC_WRITEANDEXIT => ExitEditor(false),
            _ => _activeWindow.HandleCommand(command)
        };
        if (flags.HasFlag(RenderHint.CURSOR_STATUS)) {
            UpdateCursorPosition();
            flags &= ~RenderHint.CURSOR_STATUS;
        }
        if (flags.HasFlag(RenderHint.REFRESH)) {
            StatusBar.Refresh();
            _activeWindow.Refresh();
            flags &= ~RenderHint.REFRESH;
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
    /// Add a window to the window list. This will not make the window
    /// active.
    /// </summary>
    public void AddWindow(Window theWindow) {
        if (theWindow.Buffer.NewFile) {
            string message = theWindow.Buffer.Filename == string.Empty ?
                Edit.NewFile :
                string.Format(Edit.NewFileWarning, theWindow.Buffer.Name);
            StatusBar.Message(message);
        }
        _windowList.Add(theWindow);
        SetWindowViewport(theWindow);
    }

    /// <summary>
    /// Select the next window in the specified direction in the window list.
    /// </summary>
    /// <param name="direction">Direction</param>
    /// <returns>Render hint</returns>
    private RenderHint SelectWindow(int direction) {
        if (_windowList.Count == 1) {
            StatusBar.Message(Edit.NoOtherBuffers);
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
        _activeWindow.Refresh();
        UpdateCursorPosition();
    }

    /// <summary>
    /// Edit a file in a new window, or switch to the file in an existing window.
    /// </summary>
    /// <param name="command">Editing command</param>
    /// <returns>Render hint</returns>
    private RenderHint EditFile(Command command) {
        if (command.GetFilename(Edit.File, out string inputValue)) {
            FileInfo fileInfo = new FileInfo(inputValue);
            inputValue = fileInfo.FullName;

            Window newWindow = _windowList.FirstOrDefault(window => window.Buffer.Filename == inputValue);
            if (newWindow == null) {
                newWindow = new Window(new Buffer(inputValue));
                AddWindow(newWindow);
            }
            _activeWindow = newWindow;
            _activeWindow.Refresh();
            UpdateCursorPosition();
        }
        return RenderHint.NONE;
    }

    /// <summary>
    /// Close the current window. You cannot close the window if this is
    /// the last window in the list.
    /// </summary>
    /// <returns>Render hint</returns>
    private RenderHint CloseWindow() {
        if (_windowList.Count == 1) {
            return RenderHint.NONE;
        }
        if (_activeWindow.Buffer.Modified) {
            char[] validInput = { 'y', 'n', 'w' }; 
            if (StatusBar.Prompt(Edit.ThisBufferHasNotBeenSaved, validInput, 'n', out char inputChar)) {
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
    /// Toggle display of the clock on the status bar.
    /// </summary>
    /// <returns>Render hint</returns>
    private static RenderHint ToggleClock() {
        StatusBar.ShowClock = !StatusBar.ShowClock;
        Config.ShowClock = StatusBar.ShowClock;
        return RenderHint.NONE;
    }

    /// <summary>
    /// Change the colour configuration.
    /// </summary>
    /// <param name="command">Editing command</param>
    /// <returns>Render hint</returns>
    private static RenderHint ConfigureColours(Command command) {
        if (!GetColourInput(command, Edit.BackgroundColourNumber, out int backgroundColour)) {
            return RenderHint.NONE;
        }
        if (!GetColourInput(command, Edit.ForegroundColourNumber, out int foregroundColour)) {
            return RenderHint.NONE;
        }
        if (foregroundColour == backgroundColour) {
            StatusBar.Error(Edit.InvalidColour);
            return RenderHint.NONE;
        }
        if (!GetColourInput(command, Edit.SelectedTitleColourNumber, out int selectedTitleColour)) {
            return RenderHint.NONE;
        }
        if (!GetColourInput(command, Edit.NormalTextColourNumber, out int normalMessageColour)) {
            return RenderHint.NONE;
        }
        if (!GetColourInput(command, Edit.ErrorTextColourNumber, out int errorMessageColour)) {
            return RenderHint.NONE;
        }
        Config.BackgroundColour = backgroundColour.ToString();
        Config.ForegroundColour = foregroundColour.ToString();
        Config.SelectedTitleColour = selectedTitleColour.ToString();
        Config.NormalMessageColour = normalMessageColour.ToString();
        Config.ErrorMessageColour = errorMessageColour.ToString();
        Config.Save();
        return RenderHint.REFRESH;
    }

    /// <summary>
    /// Input a colour index as part of the colour command.
    /// </summary>
    /// <param name="command">Editing command</param>
    /// <param name="prompt">Prompt to display</param>
    /// <param name="colourValue">Output value</param>
    /// <returns>True if the output value is valid, false otherwise.</returns>
    private static bool GetColourInput(Command command, string prompt, out int colourValue) {
        if (!command.GetNumber(prompt, out colourValue)) {
            return false;
        }
        if (colourValue < 0 || colourValue > Colours.MaxColourIndex) {
            StatusBar.Error(string.Format(Edit.InvalidColourIndex, Colours.MaxColourIndex));
            return false;
        }
        return true;
    }

    /// <summary>
    /// Show details of the file in the buffer on the status bar.
    /// </summary>
    /// <returns>Render hint</returns>
    private RenderHint ShowDetails() {
        StatusBar.Message(Edit.File + _activeWindow.Buffer.Filename + (_activeWindow.Buffer.Modified ? "*" : ""));
        return RenderHint.NONE;
    }

    /// <summary>
    /// Run a user specified command with optional parameters
    /// </summary>
    /// <returns>Render hint</returns>
    private RenderHint RunCommand() {
        RenderHint flags = RenderHint.NONE;
        string inputValue = string.Empty;
        if (StatusBar.PromptForInput(Edit.CommandPrompt, ref inputValue, false)) {
            Parser parser = new Parser(inputValue);
            KeyCommand commandId = KeyMap.MapCommandNameToCommand(parser.NextWord());
            if (commandId == KeyCommand.KC_NONE) {
                StatusBar.Message(Edit.UnknownCommand);
            }
            else {
                flags = HandleCommand(new Command {
                    Id = commandId,
                    Args = parser
                });
            }
        }
        return flags;
    }

    /// <summary>
    /// Show the editor version on the status bar.
    /// </summary>
    /// <returns>Render hint</returns>
    private static RenderHint Version() {
        StatusBar.Message($"{AssemblySupport.AssemblyDescription} v{AssemblySupport.AssemblyVersion} - {AssemblySupport.AssemblyCopyright}");
        return RenderHint.NONE;
    }

    /// <summary>
    /// Start or stop keystroke recording.
    /// </summary>
    /// <returns>Render hint</returns>
    private RenderHint StartStopRecording() {
        if (StatusBar.KeystrokesMode == KeystrokesMode.NONE && _recorder.HasKeystrokeMacro) {
            char[] validInput = { 'y', 'n' };
            if (!StatusBar.Prompt(Edit.OverwriteExistingKeystrokeMacro, validInput, 'n', out char inputChar)) {
                return RenderHint.NONE;
            }
            if (inputChar == 'n') {
                return RenderHint.NONE;
            }
        }
        if (StatusBar.KeystrokesMode == KeystrokesMode.RECORDING) {
            StatusBar.KeystrokesMode = KeystrokesMode.NONE;
            StatusBar.Message(Edit.KeystrokeMacroDefined);
        }
        else {
            StatusBar.KeystrokesMode = KeystrokesMode.RECORDING;
            StatusBar.Message(Edit.DefiningKeystrokeMacro);
        }
        return RenderHint.CURSOR_STATUS;
    }

    /// <summary>
    /// Play back a recorded macro.
    /// </summary>
    /// <returns>Render hint</returns>
    private RenderHint Playback() {
        RenderHint flags = RenderHint.NONE;
        if (StatusBar.KeystrokesMode == KeystrokesMode.RECORDING) {
            StatusBar.Error(Edit.CannotPlayback);
        }
        else if (!_recorder.HasKeystrokeMacro) {
            StatusBar.Error(Edit.NothingToPlayback);
        }
        else {
            StatusBar.KeystrokesMode = KeystrokesMode.PLAYBACK;
            foreach (string commandString in _recorder.Keystrokes) {
                Parser parser = new Parser(commandString);
                KeyCommand commandId = KeyMap.MapCommandNameToCommand(parser.NextWord());
                if (commandId != KeyCommand.KC_NONE) {
                    flags |= HandleCommand(new Command {
                        Id = commandId,
                        Args = parser
                    });
                }
            }
            StatusBar.KeystrokesMode = KeystrokesMode.NONE;
        }
        return flags;
    }

    /// <summary>
    /// Load a keystroke macro from a file.
    /// </summary>
    /// <returns>Render hint</returns>
    private RenderHint LoadRecording() {
        if (_recorder.HasKeystrokeMacro) {
            char[] validInput = { 'y', 'n' };
            if (!StatusBar.Prompt(Edit.OverwriteExistingKeystrokeMacro, validInput, 'n', out char inputChar)) {
                return RenderHint.NONE;
            }
            if (inputChar == 'n') {
                return RenderHint.NONE;
            }
        }
        string inputValue = string.Empty;
        if (StatusBar.PromptForInput(Edit.KeystrokeMacroFile, ref inputValue, true)) {
            if (!_recorder.LoadKeystrokes(inputValue)) {
                StatusBar.Error(Edit.KeystrokeMacroNotFound);
            } else {
                StatusBar.Message(Edit.LoadKeystrokeMacroSuccessful);
            }
        }
        return RenderHint.NONE;
    }

    /// <summary>
    /// Save the current keystroke macro to a file.
    /// </summary>
    /// <returns>Render hint</returns>
    private RenderHint SaveRecording() {
        string inputValue = string.Empty;
        if (StatusBar.PromptForInput(Edit.SaveKeystrokesAs, ref inputValue, true)) {
            _recorder.SaveKeystrokes(inputValue);
        }
        return RenderHint.NONE;
    }

    /// <summary>
    /// Search in the active window
    /// </summary>
    /// <param name="command">Editing command</param>
    /// <param name="forward">True if we search forward</param>
    /// <returns>Render hint</returns>
    private RenderHint Search(Command command, bool forward) {
        RenderHint flags = RenderHint.NONE;
        string inputValue = Config.LastSearchString;
        string prompt = string.Format(Edit.SearchFor, forward ? "\u2193" : "\u2191", Config.RegExpOff ? Edit.RegExpOffStatus : "");
        if (command.GetInput(prompt, ref inputValue)) {
            _search = new Search {
                SearchString = inputValue,
                RegExp = !Config.RegExpOff,
                CaseInsensitive = Config.SearchCaseInsensitive,
                Buffer = _activeWindow.Buffer,
                Forward = forward
            };
            Config.LastSearchString = inputValue;
            flags |= SearchAgain();
        }
        return flags;
    }

    /// <summary>
    /// Repeat the previous search.
    /// </summary>
    /// <returns>Render hint</returns>
    private RenderHint SearchAgain() {
        RenderHint flags = RenderHint.NONE;
        if (_search != null) {
            flags |= _activeWindow.Search(_search);
            StatusBar.Message(_search.MatchSuccess ? Edit.SearchCompleted : Edit.PatternNotFound);
        }
        return flags;
    }

    /// <summary>
    /// Toggle search case sensitivity
    /// </summary>
    /// <returns>Render hint</returns>
    private static RenderHint SearchCaseToggle() {
        Config.SearchCaseInsensitive = !Config.SearchCaseInsensitive;
        StatusBar.Message(Config.SearchCaseInsensitive ? Edit.CaseSensitivityOff : Edit.CaseSensitivityOn);
        return RenderHint.NONE;
    }

    /// <summary>
    /// Toggle regular expressions in search strings
    /// </summary>
    /// <returns>Render hint</returns>
    private static RenderHint RegExpToggle() {
        Config.RegExpOff = !Config.RegExpOff;
        StatusBar.Message(Config.RegExpOff ? Edit.RegExpOff : Edit.RegExpOn);
        return RenderHint.NONE;
    }

    /// <summary>
    /// Translate in the active window
    /// </summary>
    /// <param name="command">Editing command</param>
    /// <param name="forward">True if we search forward</param>
    /// <returns>Render hint</returns>
    private RenderHint Translate(Command command, bool forward) {
        string searchString = Config.LastTranslatePattern;
        string replacementString = Config.LastTranslateString;
        string prompt = string.Format(Edit.Pattern, forward ? "\u2193" : "\u2191", Config.RegExpOff ? Edit.RegExpOffStatus : "");
        if (!command.GetInput(prompt, ref searchString)) {
            return RenderHint.NONE;
        }
        if (!command.GetInput(Edit.Replacement, ref replacementString)) {
            return RenderHint.NONE;
        }
        _search = new Search {
            SearchString = searchString,
            ReplacementString = replacementString,
            RegExp = !Config.RegExpOff,
            CaseInsensitive = Config.SearchCaseInsensitive,
            Buffer = _activeWindow.Buffer,
            Forward = forward
        };
        Config.LastTranslatePattern = searchString;
        Config.LastTranslateString = replacementString;
        return TranslateAgain();
    }

    /// <summary>
    /// Repeat the previous translate.
    /// </summary>
    /// <returns>Render hint</returns>
    private RenderHint TranslateAgain() {
        RenderHint flags = RenderHint.NONE;
        if (_search != null) {
            _search.TranslateCount = 0;
            flags |= _activeWindow.Translate(_search);
            StatusBar.Message(_search.MatchSuccess ?
                string.Format(Edit.TranslateComplete, _search.TranslateCount) :
                Edit.PatternNotFound);
        }
        return flags;
    }

    /// <summary>
    /// Repeat a key command.
    /// </summary>
    /// <returns>Render hint</returns>
    private RenderHint Repeat() {
        RenderHint flags = RenderHint.NONE;
        if (StatusBar.PromptForRepeat(out int repeatCount, out Command command)) {
            while (repeatCount-- > 0) {
                flags |= HandleCommand(new Command(command));
            }
        }
        return flags;
    }

    /// <summary>
    /// Prompt for a new output file name for the current buffer. The new
    /// name must not conflict with any existing buffer name otherwise an error
    /// is displayed.
    /// </summary>
    /// <param name="command">Editing command</param>
    /// <returns>Render hint</returns>
    private RenderHint RenameOutputFile(Command command) {
        if (command.GetFilename(Edit.EnterNewOutputFileName, out string outputFileName)) {
            string fullFilename = new FileInfo(outputFileName).FullName;
            if (_windowList.Any(window => fullFilename.Equals(window.Buffer.Filename, StringComparison.OrdinalIgnoreCase))) {
                StatusBar.Message(Edit.InvalidOutputFilename);
            }
            else {
                _activeWindow.Buffer.Filename = outputFileName;
                foreach (Window window in _windowList.Where(window => window.Buffer == _activeWindow.Buffer)) {
                    window.ApplyRenderHint(RenderHint.TITLE);
                }
            }
        }
        return RenderHint.NONE;
    }

    /// <summary>
    /// Delete a file.
    /// </summary>
    /// <param name="command">Editing command</param>
    /// <returns>Render hint</returns>
    private static RenderHint DeleteFile(Command command) {
        string filename = command.Args.NextWord();
        if (!string.IsNullOrEmpty(filename)) {
            try {
                File.Delete(filename);
            } catch(Exception e) {
                StatusBar.Error(e.Message);
            }
        }
        return RenderHint.NONE;
    }

    /// <summary>
    /// Assign a command to a key.
    /// </summary>
    /// <param name="command">Editing command</param>
    /// <returns>Render hint</returns>
    private static RenderHint AssignToKey(Command command) {
        if (!command.GetFilename(Edit.EnterKey, out string keystroke)) {
            return RenderHint.NONE;
        }
        if (!command.GetFilename(Edit.EnterMacroName, out string commandName)) {
            return RenderHint.NONE;
        }
        if (!KeyMap.RemapKeyToCommand(keystroke, commandName)) {
            StatusBar.Error("Invalid keystroke or command name");
        }
        return RenderHint.NONE;
    }

    /// <summary>
    /// Change the current working directory.
    /// </summary>
    /// <param name="command">Editing command</param>
    /// <returns>Render hint</returns>
    private static RenderHint ChangeDirectory(Command command) {
        string newDirectory = command.Args.NextWord();
        if (string.IsNullOrEmpty(newDirectory)) {
            StatusBar.Message(Directory.GetCurrentDirectory());
        }
        else {
            try {
                Directory.SetCurrentDirectory(newDirectory);
            } catch(Exception e) {
                StatusBar.Error(e.Message);
            }
        }
        return RenderHint.NONE;
    }

    /// <summary>
    /// Toggle whether or not backup files are created when the buffer
    /// is saved.
    /// </summary>
    /// <returns>Render hint</returns>
    private static RenderHint ToggleBackup() {
        Config.BackupFile = !Config.BackupFile;
        StatusBar.Message(Config.BackupFile ? Edit.NoBackupFiles : Edit.BackupFiles);
        return RenderHint.NONE;
    }

    /// <summary>
    /// Toggle whether or not window borders are drawn.
    /// </summary>
    /// <returns>Render hint</returns>
    private RenderHint ToggleBorders() {
        Config.HideBorders = !Config.HideBorders;
        foreach (Window window in _windowList) {
            SetWindowViewport(window);
        }
        return RenderHint.REFRESH;
    }

    /// <summary>
    /// Set the viewport for the specified window.
    /// </summary>
    /// <param name="theWindow"></param>
    private static void SetWindowViewport(Window theWindow) {
        theWindow.SetViewportBounds(1, 1, Terminal.Width - 2, Terminal.Height - 3);
    }

    /// <summary>
    /// Exit the editor, saving any buffers if required. If prompt is
    /// TRUE, we prompt whether to save or exit without saving. If prompt
    /// is FALSE, we just save all modified buffers and exit.
    /// </summary>
    /// <param name="prompt">True to display a confirmation prompt, false to just save and exit</param>
    /// <returns>Render hint</returns>
    private RenderHint ExitEditor(bool prompt) {
        RenderHint flags = RenderHint.EXIT;
        bool writeBuffers = !prompt;
        Buffer [] modifiedBuffers = _windowList.Where(w => w.Buffer.Modified).Select(b => b.Buffer).ToArray();
        if (prompt) {
            if (modifiedBuffers.Any()) {
                char[] validInput = { 'y', 'n', 'w' };
                if (StatusBar.Prompt(string.Format(Edit.ModifiedBuffers, modifiedBuffers.Length), validInput, 'n', out char inputChar)) {
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