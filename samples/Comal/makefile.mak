JFLAGS=-w:2 -debug
DROP=..\..\drop
JCOMAL=$(DROP)\jcomal.exe $(JFLAGS)

.SUFFIXES: .lst

INCLUDE "comal.mak"

all: $(SOURCES)

$(SOURCES): jcomlib.dll jcomallib.dll $(DROP)/jcomal.exe $(DROP)/ccompiler.dll

jcomlib.dll: $(DROP)\jcomlib.dll
	copy $(DROP)\jcomlib.dll .

jcomallib.dll: $(DROP)\jcomallib.dll
	copy $(DROP)\jcomallib.dll .

.lst.exe:
	$(JCOMAL) $<

clean:
	-del /q *.exe *.dll *.pdb *.dll.mdb *.exe.mdb *.out 
	-del /q VISITORFILE FORT.*
