JFLAGS=-w:2 -debug
DROP=../../drop
JFOR=mono $(DROP)/jfor.exe $(JFLAGS)

include tests.mak

all: $(ALLTESTS) $(INTTESTS) $(ALLDATATESTS) $(INTDATATESTS)

failures: $(FAILTESTS)

run: $(ALLTESTS) $(ALLDATATESTS)
	-rm run.out FORT.*
	-for f in $(ALLTESTS); do mono ./$$f; done >>run.out
	-for f in $(ALLDATATESTS); do mono ./$$f < ./$${f%.exe}.DAT; done>>run.out
	-grep "\sFAIL\s" run.out

runall: $(ALLTESTS) $(ALLDATATESTS) $(INTTESTS) $(INTDATATESTS)
	-rm run.out FORT.*
	-for f in $(ALLTESTS); do mono ./$$f; done >>run.out
	-for f in $(ALLDATATESTS); do mono ./$$f < ./$${f%.exe}.DAT; done>>run.out
	-for f in $(INTTESTS); do mono ./$$f; done
	-for f in $(INTDATATESTS); do mono ./$$f < ./$${f%.exe}.DAT; done
	-grep "\sFAIL\s" run.out

$(ALLTESTS) $(INTTESTS) $(ALLDATATESTS) $(INTDATATESTS): comlib.dll forlib.dll $(DROP)/com.dll $(DROP)/for.exe

comlib.dll: $(DROP)/comlib.dll
	cp $(DROP)/comlib.dll .

forlib.dll: $(DROP)/forlib.dll
	cp $(DROP)/forlib.dll .

%.exe : %.FOR
	$(JFOR) $<

clean:
	-rm *.exe *.pdb *.exe.mdb *.out FORT.*

