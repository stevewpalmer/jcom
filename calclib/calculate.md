# Cell Calculation Algorithm

The cell calculation algorithm in calc relies on the use of a directed
acyclic graph (DAG) maintained in the workbook. The graph stores a collection
of edges for every cell that either references another cell or is referenced
by another cell.

Cells that reference other cells have dependencies. And example is a formula
such as in cell C2:
```
C2: =B4*SUM(A1:A3)
```
which has four precedents: cells B4, A1, A2 and A3. Each of those four
cells are precedent cells in that they are referenced by at least one other
cell.

The graph thus will have the following edges:
```
C2<->B4
C2<->A1
C2<->A2
C2<->A3
```
The cells on the right are precedent cells. Changes to precedent cells will
trigger an update on the dependent cell. Thus, a change to cell A2 will
trigger a recalculation of the formula at C2 and the value displayed in
that cell.

This is obviously a simplistic example. A more complex example, and thus a
more complex graph, would be when formula cells reference other formula
cells such as this:
```
C1:=SUM(A1:A3)
C2:=C1+AVG(SUM(B1:B3))
C3:=C1+C2
```
The resulting graph will have edges leading from C2 to C1 and from C3 to
both C1 and C3. Now cell A2 is a precedent of not just C1 but of C2 and C3
and thus all three cells will need to be recalculated if A2 changes. A
key aspect of the graph is that the order of calculation matters. Cell C1
needs to be calculated first, followed by C2 and then C3 in that order.
This is achieved by following the edges from A2 to C1, then C1 to C2 and
C2 to C3.

### Updating The Graph

There are two main operations on the spreadsheets that will require changes
to the graph.

1. Changing a cell, including editing, adding or removing a formula.
2. Adding or removing rows and columns.

Changing a cell necessitates removing all existing dependencies from the
cell and thus removal of all edges leading out from the graph node for that
cell and cells leading in. However, we do not remove the dependent cell as
another cell can still be dependent on the value of that cell even if it is
no longer a formula or is blank.

Adding or removing rows and columns is more significant in that this causes
many cell addresses to potentially change. For example, in the example above,
removing column A will invalidate the formula in both C2 and C3. In this
example:
```
A1:=SUM(C1:C3)
```
removing column B will cause the formula to be adjusted to:
```
A1:=SUM(B1:B3)
```
and thus the dependencies for formulas on every adjusted cell will need to be
recalculated. This is achieved in the same way as when we change a cell since
the effect of the adjustment is identical to changing the formula with new
cell references.