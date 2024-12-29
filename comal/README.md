# Comal

This folder contains the source files for a simple Comal interpreter/compiler
that follows the Comal standard. See the TODO.md file for a list of
features not yet implemented.

To run Comal in interative mode, type:

`comal`

from the shell. From there you can write, save and load Comal programs
and run them within the interpreter. Programs saved from the interpreter
are stored in tokenised format which can only be read by loading back into
the interpreter. However, you can write Comal programs in the Editor and
save them as .lst files. These can be read into Comal using the `enter`
command.

To run the Comal compiler, specify a Comal program on the command line:

`comal helloworld.cml`

`comal helloworld.lst`

The compiler will accept either a tokenised source file or a plaintext
source file.

The full list of command line arguments can be viewed by specifying the
--help command line option:

```
$ comal --help
Comal 1.0.0 (c) Steven Palmer 2013-2024
comal [options] [source-files]
--help              Lists all compiler options (short: -h)
--version           Display compiler version (short: -v)
--strict            Enable strict mode
--ide               Enable IDE mode
--debug             Generate debugging information
--warn:NUM          Sets warning level, the default is 4 (short: -w)
--warnaserror       Treats all warnings as errors
--dump              Output compiler debugging information to a file
--noinline          Don't inline intrinsic calls
--run               Run the executable if no errors
--out:FILE          Specifies output executable name (short: -o)
```

You can also compile and run Comal programs from within the Editor. Just
ensure that the file being edited has the .lst extension and then press
Alt+F10 to compile and run the program. If there are any errors, the
errors will be displayed in the Editor with the cursor set to the
offending line. You can step through each error with the F3 key.
