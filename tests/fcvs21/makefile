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

$(ALLTESTS) $(INTTESTS) $(ALLDATATESTS) $(INTDATATESTS): jcomlib.dll jforlib.dll  $(DROP)/ccompiler.dll $(DROP)/jfor.exe

jcomlib.dll: $(DROP)/jcomlib.dll
	cp $(DROP)/jcomlib.dll .

jforlib.dll: $(DROP)/jforlib.dll
	cp $(DROP)/jforlib.dll .

%.exe : %.FOR
	$(JFOR) $<

clean:
	-rm *.exe *.pdb *.exe.mdb *.out FORT.*

