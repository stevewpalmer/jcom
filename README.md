# JCom Compilers

This repository contains the source code for simple Fortran and Comal compilers.

JCom is an experiment in creating a compiler for various pre-1980s languages in C# using the .NET Reflection classes to emit the output code and to define a single back-end and run-time for all these languages. Currently only Fortran and Comal are implemented but the plan is to add support for further languages starting with C and BCPL.

## Fortran

JFor is a subset of Fortran 77 with various pieces not yet implemented. Consequently this is very much a work in progress. See the TODO.md for more details.

## Comal

Comal is both an interactive compiler and full compiler for the Comal Standard language. It can be run interactively and provides an input environment for writing, running, loading and saving simple Comal programs. The compiler is invoked by specifying either the source listing (.lst) or the tokenised file (.cml) as input and produces a stand-alone executable.
