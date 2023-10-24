JFLAGS=-w:2 --debug --run
DROP=../../drop/bin
JFOR=$(DROP)/for $(JFLAGS)

include tests.mak

all: $(ALLTESTS) $(INTTESTS) $(ALLDATATESTS) $(INTDATATESTS)

failures: $(FAILTESTS)
	-rm run.out FORT.*
	-for f in $(FAILTESTS); do $(JFOR) $$f; done >>run.out
	-grep "\sFAIL\s" run.out

run: $(ALLTESTS)
	-rm run.out FORT.*
	-for f in $(ALLTESTS); do $(JFOR) $$f; done >>run.out
	-for f in $(ALLDATATESTS); do $(JFOR) $$f < ./$${f%.FOR}.dat; done>>run.out
	-grep "\sFAIL\s" run.out

runall: $(ALLTESTS) $(ALLDATATESTS) $(INTTESTS) $(INTDATATESTS)
	-rm run.out FORT.*
	-for f in $(ALLTESTS); do $(JFOR) $$f; done >>run.out
	-for f in $(ALLDATATESTS); do $(JFOR) $$f < ./$${f%.FOR}.DAT; done>>run.out
	-for f in $(INTTESTS); do $(JFOR) $$f; done
	-for f in $(INTDATATESTS); do $(JFOR) $$f < ./$${f%.FOR}.DAT; done
	-grep "\sFAIL\s" run.out

$(ALLTESTS) $(INTTESTS) $(ALLDATATESTS) $(INTDATATESTS): $(DROP)/com.dll $(DROP)/for

clean:
	-rm *.out FORT.*

