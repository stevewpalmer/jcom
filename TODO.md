# To Do List

Lots of bugs to fix. The below are the things that need to be done in
decreasing order of priority.

Note that as of 28th November 2024, these are now actively tracked on
the GitHub Issues page for greater visibility. However for source code
hosting portability, I will try and keep this file updated separately.

## Work Items

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
* Support Nested PROC/FUNC.
    * How can nested proc/func reference a parameter or variable in the parent method??
    * How do we deal with IMPORT from a PROC/FUNC we’re nested in?
    * Idea - what if we passed IMPORTed variables as hidden parameters to the nested functions?

### Editor

* Undo/Redo.
* Split windows.

### Calc

* Undo/Redo.
* Split windows.

### Code Generation

* Fix local lifetime optimisation.
* Common sub-expression optimisation:
    - eg. ((B/20)*A)+((B/20)*C)
* Peephole optimiser:
    - Add lifetime scan to optimise local storage usage.
