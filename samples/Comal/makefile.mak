JFLAGS=-w:2 -debug
DROP=..\..\drop
JCOMAL=$(DROP)\comal.exe $(JFLAGS)

.SUFFIXES: .lst

INCLUDE "comal.mak"

all: $(SOURCES)

$(SOURCES): comlib.dll comallib.dll $(DROP)/comal.exe $(DROP)/com.dll

comlib.dll: $(DROP)\comlib.dll
	copy $(DROP)\comlib.dll .

comallib.dll: $(DROP)\comallib.dll
	copy $(DROP)\comallib.dll .

.lst.exe:
	$(JCOMAL) $<
	
.lst.dll:
	$(JCOMAL) $<

clean:
	-del /q *.exe *.dll *.pdb *.dll.mdb *.exe.mdb *.out 
	-del /q VISITORFILE FORT.*
