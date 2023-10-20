# JCom

This repository contains the source code for a simple console
based operating system similar to those from the 1970s and 80s. It
includes a basic shell interface, a text editor, a Comal compiler
and interpreter and a Fortran compiler. Other features are being
added over time.

This is very much a personal project by one who grew up during
the 1980s and for who my first experience with computers was via
the rudimentary console based interfaces. Compared with modern
operating systems this is extremely basic and limited but I still
feel that this is a good way for those unused to computers to get
started with learning to write simple programs. In addition, by
eliminating network access, it avoids almost all the security
issues inherent in today's systems.

The solution currently includes four projects:

### Shell

This is the start-up project and implements a very simple console
based command line shell similar to MS-DOS. At the moment is only
provides a means for running the other programs but I hope to
expand it gradually while still keeping it very simple.

### Edit

This folder contains the source files for a simple text editor
based on the DOS Brief editor from the 1990s.

### Accounts

This is a very simple disk based personal finance account management
program originally written in FreeBasic [here](https://github.com/stevewpalmer/accounts).
This version is a port of that to C# and integrated with the rest of
the project.

### Fortran

This is a subset of Fortran 77 with various pieces not yet
implemented. Consequently this is very much a work in progress.
See the TODO.md for more details.

### Comal

Comal is both an interactive compiler and full compiler
for the Comal Standard language. It can be run interactively
and provides an input environment for writing, running,
loading and saving simple Comal programs. The compiler is
invoked by specifying either the source listing (.lst) or
the tokenised file (.cml) as input.
