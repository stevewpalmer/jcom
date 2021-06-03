# NIST Fortran 77 test suite

This folder contains the source files for the NIST Fortran 77 test suite plus makefiles required to run them. All tests should pass when executed with

`make run`

Other tests are included but will not yet pass. See the TODO.md in the root of this repository for a list of bug fixes needed to enable them to pass. When they successfully pass, move these to the ALLTESTS list in tests.mak so they're included in the standard passing list.

Use:

`make clean`

to clean up this folder prior to each run or commit.