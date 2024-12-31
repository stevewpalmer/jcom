# Fortran

This folder contains the source files for a simple Fortran compiler that
implements most of the F77 standard and some aspects of Fortran 90.
See the TODO.md file for a list of features not yet implemented.

To run the Fortran compiler, specify a Fortran source file on the command
line:

`fort helloworld.f`

The full list of command line arguments can be viewed by specifying the
--help command line option:

```
$ fort --help
Fortran Compiler 1.0.0 (c) Steven Palmer 2013-2025
fort [options] [source-files]
   --help              Lists all compiler options (short: -h)
   --version           Display compiler version (short: -v)
   --backslash         Permit C style escapes in strings
   --f77               Compile Fortran 77 source (default)
   --f90               Compile Fortran 90 source
   --debug             Generate debugging information
   --warn:NUM          Sets warning level, the default is 4 (short: -w)
   --warnaserror       Treats all warnings as errors
   --dump              Output compiler debugging information to a file
   --noinline          Don't inline intrinsic calls
   --run               Run the executable if no errors
   --out:FILE          Specifies output executable name (short: -o)
```

You can also compile and run Fortran programs from within the Editor. Just
ensure that the file being edited has the .f or .f90 extension and then
press Alt+F10 to compile and run the program. If there are any errors,
the errors will be displayed in the Editor with the cursor set to the
offending line. You can step through each error with the F3 key.
