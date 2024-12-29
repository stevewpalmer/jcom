JFLAGS=-w:4 --debug
DROP=../../drop/bin
JCOMAL=$(DROP)/comal $(JFLAGS)

SOURCES=append.exe and.exe and2.exe standard.dll substrings.exe filetest.exe using.exe \
	isprime.exe btree.exe hammurabi.exe autorep.exe abs.exe hello.exe input.exe \
    lower.exe time.exe trap2.exe maze.exe

BINARIES=comlib.dll comallib.dll $(DROP)/comal $(DROP)/com.dll

all: $(SOURCES)

$(SOURCES): $(BINARIES)

comlib.dll: $(DROP)/comlib.dll
	cp $(DROP)/comlib.dll .

comallib.dll: $(DROP)/comallib.dll
	cp $(DROP)/comallib.dll .

%.dll %.exe : %.lst
	$(JCOMAL) $<

clean:
	-rm -f *.exe *.dll *.runtimeconfig.json *.out
	-rm -f VISITORFILE