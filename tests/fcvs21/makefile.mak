JFLAGS=-w:2 -debug
DROP=..\..\drop
JFOR=$(DROP)\jfor.exe $(JFLAGS)

INCLUDE "tests.mak"

all: $(ALLTESTS) $(INTTESTS) $(ALLDATATESTS) $(INTDATATESTS) $(BUGTESTS)

failures: $(FAILTESTS)

run: $(ALLTESTS) $(ALLDATATESTS) $(BUGTESTS)
	-del /q run.out FORT.*
	-for %f in ($(ALLTESTS) $(BUGTESTS)) do @%f >>run.out
	-for %f in ($(ALLDATATESTS)) do @%f < %~nf.DAT; >>run.out
	-findstr /c:" FAIL " run.out

runall: $(ALLTESTS) $(ALLDATATESTS) $(INTTESTS) $(INTDATATESTS) $(BUGTESTS)
	-del /q run.out FORT.*
	-for %f in ($(ALLTESTS) $(BUGTESTS)) do @%f >>run.out
	-for %f in ($(INTTESTS)) do @%f
	-for %f in ($(DATATESTS)) do @%f < %~nf.DAT;
	-for %f in ($(INTDATATESTS)) do @%f < %~nf.DAT;
	-findstr /c:" FAIL " run.out

$(ALLTESTS) $(INTTESTS) $(ALLDATATESTS) $(INTDATATESTS) $(BUGTESTS): jcomlib.dll  $(DROP)/ccompiler.dll $(DROP)/jfor.exe

jcomlib.dll: $(DROP)/jcomlib.dll
	copy $(DROP)\jcomlib.dll .

.FOR.exe:
	$(JFOR) $<

clean:
	-del /q *.exe *.pdb *.exe.mdb *.out FORT.*

