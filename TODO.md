# To Do List

Lots of bugs to fix. The below are the things that need to be done in
decreasing order of priority.

## Work Items

### Editor
* Replace.

### Fortran
* BLOCK DATA (FM500, FM503, FM506)
* Assigned FORMAT label (FM111, FM252)
* Tokeniser (FM200, FM010, FM011)
* Substring assignment (FM903, FM710)
* EQUIVALENCE - probably will never do this
* Alternate ENTRY (FM519, FM507)

### Comal
* CHAINing
* Non-constant string lengths in DIM...OF
* Console error handling on Linux

### Cobol
* Implement

## Bug Fixes

### Fortran
* Fix failing ArrayVerifyDimensions1 unit test.

### Comal
* USE libraries not working on Windows.
* GET$ is skipping over newlines.
* Fix USE statement on Windows
* Fix Nested PROC/FUNC.
  * How can nested proc/func reference a parameter or variable in the parent method??
  * How do we deal with IMPORT from a PROC/FUNC we’re nested in?
  * Idea - what if we passed IMPORTed variables as hidden parameters to the nested functions?
* Debug Info
  * For some reason, local symbols don't show up in the VS Local pane.
  * On Visual Studio Mac, the debugger is unable to find the source code.
  * Single-stepping gets stuck on the first line for several steps in ABS.LST

### Code Generation
* Fix local lifetime optimisation.
* Common sub-expression optimisation:
  - eg. ((B/20)*A)+((B/20)*C)
* Peephole optimiser:
  - Add lifetime scan to optimise local storage usage.
