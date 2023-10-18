# Fortran

This folder contains the source files for a simple Fortran compiler that
implements most of the F77 standard and some aspects of Fortran 90.
See the TODO.md file for a list of features not yet implemented.

To run the Fortran compiler, specify a Fortran source file on the command
line:

`for helloworld.f`

At the moment, the compiler cannot generate a stand-alone application due
to limitations of .NET Core 7. Thus to run the compiled program, you need
to use the --run command line option which will both compile the source
and then run if there are no errors.

`for --run helloworld.f`

The full list of command line arguments can be viewed by specifying the
--help command line option:

```
$ for --help
Fortran Compiler 1.0.0 (c) Steven Palmer 2013-2023
for [options] [source-files]
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

The --out option will not yet work until the ability to generate a
stand-alone application is restored.

You can also compile and run Fortran programs from within the Editor. Just
ensure that the file being edited has the .f or .f90 extension and then
press Alt+F10 to compile and run the program. If there are any errors,
the errors will be displayed in the Editor with the cursor set to the
offending line. You can step through each error with the F3 key.
