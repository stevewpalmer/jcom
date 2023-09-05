JFLAGS=-w:4 -strict
DROP=../../drop
JCOMAL=mono $(DROP)/comal.exe $(JFLAGS)

include comal.mak

BINARIES=comlib.dll comallib.dll $(DROP)/comal.exe $(DROP)/com.dll

all: $(SOURCES)

$(SOURCES): $(BINARIES)

jcomlib.dll: $(DROP)/jcomlib.dll
	cp $(DROP)/jcomlib.dll .

jcomallib.dll: $(DROP)/jcomallib.dll
	cp $(DROP)/jcomallib.dll .

%.dll %.exe : %.lst
	$(JCOMAL) $<

clean:
	-rm *.exe *.dll *.pdb *.dll.mdb *.exe.mdb *.out 
	-rm VISITORFILE

