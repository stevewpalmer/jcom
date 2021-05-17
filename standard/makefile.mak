JFLAGS=-w:2 -debug
DROP=..\drop
JFOR=$(DROP)/jfor.exe $(JFLAGS)
JCOMAL=$(DROP)/jcomal.exe $(JFLAGS)

SOURCES=append.exe and.exe and2.exe standard.dll substrings.exe filetest.exe using.exe \
	isprime.exe btree.exe hammurabi.exe

BINARIES=jcomlib.dll jcomallib.dll jforlib.dll $(DROP)/jcomal.exe $(DROP)/ccompiler.dll $(DROP)/jfor.exe

all: $(SOURCES)

$(SOURCES): $(BINARIES)

jcomlib.dll: $(DROP)\jcomlib.dll
	copy $(DROP)\jcomlib.dll .

jforlib.dll: $(DROP)\jforlib.dll
	copy $(DROP)\jforlib.dll .

jcomallib.dll: $(DROP)\jcomallib.dll
	copy $(DROP)\jcomallib.dll .

%.dll %.exe : %.lst
	$(JCOMAL) $<

%.exe : %.FOR
	$(JFOR) $<

clean:
	-del /q *.exe *.dll *.pdb *.dll.mdb *.exe.mdb *.out 
	-del /q VISITORFILE FORT.*
