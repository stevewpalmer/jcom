JFLAGS=-w:4
DROP=../../drop
JCOMAL=mono $(DROP)/jcomal.exe $(JFLAGS)

SOURCES=append.exe and.exe and2.exe standard.dll substrings.exe filetest.exe using.exe \
	isprime.exe btree.exe hammurabi.exe autorep.exe abs.exe hello.exe input.exe \
    lower.exe time.exe trap2.exe

BINARIES=jcomlib.dll jcomallib.dll $(DROP)/jcomal.exe $(DROP)/ccompiler.dll

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

