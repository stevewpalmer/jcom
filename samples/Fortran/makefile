JFLAGS=-w:4
DROP=../../drop
JFOR=mono $(DROP)/jfor.exe $(JFLAGS)

INCLUDE "fortran.mak"

BINARIES=comlib.dll forlib.dll $(DROP)/com.dll $(DROP)/for.exe

all: $(SOURCES)

$(SOURCES): $(BINARIES)

comlib.dll: $(DROP)/comlib.dll
	cp $(DROP)/comlib.dll .

forlib.dll: $(DROP)/forlib.dll
	cp $(DROP)/forlib.dll .

%.exe : %.f
	$(JFOR) $<

%.exe : %.f90
	$(JFOR) $<

clean:
	-rm *.exe *.dll *.pdb *.dll.mdb *.exe.mdb *.out 
