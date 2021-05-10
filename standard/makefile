JFLAGS=-w:2 -debug
DROP=../drop
JFOR=mono $(DROP)/jfor.exe $(JFLAGS)
JCOMAL=mono $(DROP)/jcomal.exe $(JFLAGS)

all: append.exe and.exe and2.exe standard.dll substrings.exe jcomlib.dll jcomallib.dll jforlib.dll

jcomlib.dll: $(DROP)/jcomlib.dll
	cp $(DROP)/jcomlib.dll .

jforlib.dll: $(DROP)/jforlib.dll
	cp $(DROP)/jforlib.dll .

jcomallib.dll: $(DROP)/jcomallib.dll
	cp $(DROP)/jcomallib.dll .

%.exe : %.lst
	$(JCOMAL) $<

%.exe : %.FOR
	$(JFOR) $<

clean:
	-rm *.exe *.pdb *.exe.mdb *.out FORT.*

