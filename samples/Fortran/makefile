JFLAGS=-w:4
DROP=../../drop
JFOR=mono $(DROP)/jfor.exe $(JFLAGS)

INCLUDE "fortran.mak"

BINARIES=jcomlib.dll jforlib.dll $(DROP)/ccompiler.dll $(DROP)/jfor.exe

all: $(SOURCES)

$(SOURCES): $(BINARIES)

jcomlib.dll: $(DROP)/jcomlib.dll
	cp $(DROP)/jcomlib.dll .

jforlib.dll: $(DROP)/jforlib.dll
	cp $(DROP)/jforlib.dll .

%.exe : %.f
	$(JFOR) $<

%.exe : %.f90
	$(JFOR) $<

clean:
	-rm *.exe *.dll *.pdb *.dll.mdb *.exe.mdb *.out 
