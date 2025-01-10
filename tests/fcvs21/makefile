JFLAGS=-w:2 --debug
DROP=../../drop/bin
JFOR=$(DROP)/fort $(JFLAGS)
JRUN=dotnet

include tests.mak

all: $(ALLTESTS) $(INTTESTS) $(ALLDATATESTS) $(INTDATATESTS)

failures: $(FAILTESTS)
	-rm run.out FORT.*
	-for f in $(FAILTESTS); do $(JRUN) $$f; done >>run.out
	-grep "\sFAIL\s" run.out

run: $(ALLTESTS) $(ALLDATATESTS)
	-rm run.out FORT.*
	-for f in $(ALLTESTS); do $(JRUN) $$f; done >>run.out
	-for f in $(ALLDATATESTS); do $(JRUN) $$f < ./$${f%.exe}.dat; done>>run.out
	-grep "\sFAIL\s" run.out

runall: $(ALLTESTS) $(ALLDATATESTS) $(INTTESTS) $(INTDATATESTS)
	-rm run.out FORT.*
	-for f in $(ALLTESTS); do $(JRUN) $$f; done >>run.out
	-for f in $(ALLDATATESTS); do $(JRUN) $$f < ./$${f%.exe}.DAT; done>>run.out
	-for f in $(INTTESTS); do $(JRUN) $$f; done
	-for f in $(INTDATATESTS); do $(JRUN) $$f < ./$${f%.exe}.DAT; done
	-grep "\sFAIL\s" run.out

$(ALLTESTS) $(INTTESTS) $(ALLDATATESTS) $(INTDATATESTS): comlib.dll forlib.dll $(DROP)/fort

comlib.dll: $(DROP)/comlib.dll
	cp $(DROP)/comlib.dll .

forlib.dll: $(DROP)/forlib.dll
	cp $(DROP)/forlib.dll .

%.exe : %.FOR
	$(JFOR) $<

clean:
	-rm *.out *.exe *.runtimeconfig.json FORT.*

