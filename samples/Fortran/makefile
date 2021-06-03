JFLAGS=-w:4
DROP=../../drop
JFOR=mono $(DROP)/jfor.exe $(JFLAGS)

SOURCES=cnsume.exe curves.exe date.exe datest.exe euclid.exe f2ccalc.exe heron.exe \
        loan.exe petrol.exe reduce.exe stats.exe triang.exe vowels.exe bottles.exe

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
