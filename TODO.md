# To Do List

Lots of bugs to fix. The below are the things that need to be done in
decreasing order of priority.

## Work Items

JFor
* Assigned FORMAT label (FM111, FM252)
* BLOCK DATA
* Tokeniser (FM200, FM010, FM011)
* Substring assignment (FM903, FM710)
* EQUIVALENCE
* Alternate ENTRY (FM519, FM507)

JComal
* CHAINing
* Non-constant string lengths in DIM...OF
* AND...THEN, OR...THEN expressions

## Bug Fixes

JFor
* Handle missing PROGRAM at start.
* Fix failing ArrayVerifyDimensions1 unit test.

JComal:
* Finish dynamic array support.
  * Verify 3D dynamic arrays.
  * Verify dynamic arrays with ranges.
  * Add unit tests.
* CLOSED methods:
  * Create local symbol table for parameters AND variables.
  * Search local and import symbols only.
  * New variables go into local symbol table.
* OPEN methods:
  * Create local symbol table for parameters only.
  * Search global stack only.
  * New variables go into global stack only.
* FOR variables.
  * The variable itself is local to both CLOSED and OPEN methods.
  * Other variables are as per the rules above.
  * Add to the local table we create.
* Nested PROC/FUNC.
  * How can nested proc/func reference a parameter or variable in the parent method??
  * How do we deal with IMPORT from a PROC/FUNC we’re nested in?
  * Idea - what if we passed IMPORTed variables as hidden parameters to the nested functions?

Codegen:
* Fix local lifetime optimisation.
* Common sub-expression optimisation:
  - eg. ((B/20)*A)+((B/20)*C)
* Peephole optimiser:
  - Add lifetime scan to optimise local storage usage.

Documentation:
* Begin writing description of architecture.
