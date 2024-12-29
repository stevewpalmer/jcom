JFLAGS=-w:4 --debug
DROP=../../drop/bin
JFOR=$(DROP)/fort $(JFLAGS)

SOURCES=date.exe datest.exe euclid.exe f2ccalc.exe heron.exe \
	loan.exe petrol.exe reduce.exe stats.exe triang.exe vowels.exe bottles.exe

BINARIES=comlib.dll forlib.dll $(DROP)/com.dll $(DROP)/fort

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
	-rm -f *.exe *.dll *.runtimeconfig.json *.out
