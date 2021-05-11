JFLAGS=-w:2 -debug
DROP=../drop
JFOR=mono $(DROP)/jfor.exe $(JFLAGS)
JCOMAL=mono $(DROP)/jcomal.exe $(JFLAGS)

SOURCES=append.exe and.exe and2.exe standard.dll substrings.exe filetest.exe using.exe

BINARIES=jcomlib.dll jcomallib.dll jforlib.dll $(DROP)/jcomal.exe $(DROP)/ccompiler.dll $(DROP)/jfor.exe

all: $(SOURCES)

$(SOURCES): $(BINARIES)

jcomlib.dll: $(DROP)/jcomlib.dll
	cp $(DROP)/jcomlib.dll .

jforlib.dll: $(DROP)/jforlib.dll
	cp $(DROP)/jforlib.dll .

jcomallib.dll: $(DROP)/jcomallib.dll
	cp $(DROP)/jcomallib.dll .

%.dll %.exe : %.lst
	$(JCOMAL) $<

%.exe : %.FOR
	$(JFOR) $<

clean:
	-rm *.exe *.dll *.pdb *.dll.mdb *.exe.mdb *.out FORT.*

