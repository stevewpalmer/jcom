JFLAGS=-w:2 -debug
DROP=..\..\drop
JFOR=$(DROP)\jfor.exe $(JFLAGS)

INCLUDE "fortran.mak"

all: $(SOURCES)

$(SOURCES): jcomlib.dll jforlib.dll $(DROP)/ccompiler.dll $(DROP)/jfor.exe

jcomlib.dll: $(DROP)\jcomlib.dll
	copy $(DROP)\jcomlib.dll .

jforlib.dll: $(DROP)\jforlib.dll
	copy $(DROP)\jforlib.dll .

.f90.exe:
	$(JFOR) $<

.f.exe:
	$(JFOR) $<

clean:
	-del /q *.exe *.dll *.pdb *.dll.mdb *.exe.mdb *.out 
	-del /q VISITORFILE FORT.*
