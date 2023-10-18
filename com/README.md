# Compiler Back-end

This folder contains the source files for the compiler back-end that
implements the parse nodes and code generation. This is required to be
distributed with the compilers themselves but not the generated executable
files.

**Note:** At the moment, the back-end does not yet generate stand-alone
executable files due to limitations of .NET Core 7. The generated code
needs to be explicitly run after compilation by the relevant compilers.
