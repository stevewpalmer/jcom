# Edit

This folder contains the source files for a simple text editor
based on the DOS Brief editor from the 1990s.

<img width="707" alt="edit" src="https://github.com/user-attachments/assets/bf281f80-8fb1-47ae-9425-661f1eab7c1c">

Run the editor from the shell with the `edit` command. By default
this opens the editor and prompts you for the name of the file to edit.
Specify one or more filenames after the command to open those files,
each in their own buffer.

`edit helloworld.f`

You can also compile and run Comal or Fortran programs from within the
Editor. Just ensure that the file being edited has the .f or .f90 extension
for a Fortran source file, or .lst for a Comal source file, and then
press Alt+F10 to compile and run the program. If there are any errors,
the errors will be displayed in the Editor with the cursor set to the
offending line. You can step through each error with the F3 key.

### Keyboard

| Command                  | Windows                  | Unix                     |
|--------------------------|--------------------------|--------------------------|
| Left                     | Left arrow               | Left arrow               |
| Right                    | Right arrow              | Right arrow              |
| Up                       | Up arrow                 | Up arrow                 |
| Down                     | Down arrow               | Down arrow               |
| Exit                     | Alt+X                    | Alt+X                    |
| Delete                   | Del                      | Del                      |
| Backspace                | Backspace                | Backspace                |
| Delete Next Word         | Alt+Backspace            | Alt+E                    |
| Delete Previous Word     | Ctrl+Backspace           | Alt+H                    |
| Write Buffer             | Alt+W                    | Alt+W                    |
| Write Buffers and Exit   | Ctrl+X                   | Ctrl+X                   |
| Change Output Filename   | Alt+O                    | Alt+O                    |
| Version                  | Alt+V                    | Alt+V                    |
| Details                  | Alt+F                    | Alt+F                    |
| Edit Buffer              | Alt+E                    | Ctrl+O                   |
| Repeat Command           | Ctrl+R                   | Ctrl+R                   |
| Record Keystrokes        | F7                       | F7                       |
| Playback Recording       | F8                       | F8                       |
| Save Recording           | Alt+F8                   | Alt+F8                   |
| Load Recording           | Alt+F7                   | Alt+F7                   |
| Command                  | F10                      | F10                      |
| Compile and Run          | Alt+F10                  | Alt+F10                  |
| Overstrike/Insert        | Insert                   | Insert                   |
| Go To Line               | Alt+G                    | Ctrl+G                   |
| Start of Line            | Home                     | Alt+Left / Home          |
| End of Line              | End                      | Alt+Right / End          |
| Start of Buffer          | Alt+Home                 | Ctrl+L                   |
| End of Buffer            | Alt+End                  | Ctrl+U                   |
| Top of Window            | Ctrl+Home                | Alt+T                    |
| Bottom of Window         | Ctrl+End                 | Alt+B                    |
| Next Word                | Ctrl+Command+Right arrow | Ctrl+Command+Right arrow |
| Previous Word            | Ctrl+Command+Left arrow  | Ctrl+Command+Left arrow  |
| Page Down                | PgDn                     | PgDn                     |
| Page Up                  | PgUp                     | PgUp                     |
| Screen Down              | Ctrl+D                   | Ctrl+D                   |
| Screen Up                | Ctrl+E                   | Ctrl+E                   |
| Line to Top of Window    | Ctrl+T                   | Ctrl+T                   |
| Line to Bottom of Window | Ctrl+B                   | Ctrl+B                   |
| Centre Screen            | Ctrl+C                   | Ctrl+W                   |
| Mark                     | Alt+M                    | Alt+M                    |
| Mark Line                | Alt+L                    | Alt+L                    |
| Mark Column              | Alt+C                    | Alt+C                    |
| Copy to Scrap            | Ctrl+OemPlus             | Ctrl+C                   |
| Cut to Scrap             | Ctrl+OemMinus            | Ctrl+X                   |
| Paste from Scrap         | Insert                   | Ctrl+V                   |
| Delete Line              | Alt+D                    | Alt+D                    |
| Delete to Start of Line  | Ctrl+K                   | Ctrl+K                   |
| Delete to End of Line    | Alt+K                    | Alt+K                    |
| Next Buffer              | Alt+N                    | Ctrl+N                   |
| Previous Buffer          | Alt+-                    | Ctrl+P                   |
| Search Forward           | F5 or Alt+S              | F5 or Alt+S              |
| Search Backward          | Alt+F5                   | Ctrl+U                   |
| Search Again             | Shift+F5                 | Ctrl+A                   |
| Translate                | F6 or Alt+T              | F6 or Alt+T              |
| Translate Backward       | Alt+F6                   | Alt+U                    |
| Translate Again          | Shift+F6                 | Alt+A                    |
| Toggle Case Search       | Ctrl+F5                  | Ctrl+S                   |
| Toggle RegEx Search      | Ctrl+F6                  | Ctrl+F                   |
