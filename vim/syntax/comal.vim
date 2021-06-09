" Vim syntax file
" Language:	COMAL
" Maintainer:	Steve Palmer <83670458+stevewpalmer@users.noreply.github.com>
" Last Change:  2021 jun 8 by Steve Palmer

" quit when a syntax file was already loaded
if exists("b:current_syntax")
  finish
endif

let s:cpo_save = &cpo
set cpo&vim

" A bunch of useful COMAL keywords
syn keyword comalStatement       append Append APPEND at At AT auto Auto
syn keyword comalStatement       AUTO  bye Bye BYE case Case CASE cat Cat
syn keyword comalStatement       CAT change Change CHANGE close Close
syn keyword comalStatement       CLOSE closed Closed CLOSED colour Colour
syn keyword comalStatement       COLOUR con Con CON create Create CREATE 
syn keyword comalStatement       cursor Cursor CURSOR data Data DATA del
syn keyword comalStatement       Del DEL delete Delete DELETE dim Dim DIM
syn keyword comalStatement       dir Dir DIR display Display DISPLAY do
syn keyword comalStatement       Do DO edit Edit EDIT elif Elif ELIF else
syn keyword comalStatement       Else ELSE end End END endcase Endcase
syn keyword comalStatement       ENDCASE endfor Endfor ENDFOR endfunc
syn keyword comalStatement       Endfunc ENDFUNC endif Endif ENDIF
syn keyword comalStatement       endloop Endloop ENDLOOP endmodule
syn keyword comalStatement       Endmodule ENDMODULE endproc Endproc
syn keyword comalStatement       ENDPROC endtrap Endtrap ENDTRAP endwhile
syn keyword comalStatement       Endwhile ENDWHILE enter Enter ENTER exec
syn keyword comalStatement       Exec EXEC exit Exit EXIT export Export
syn keyword comalStatement       EXPORT external External EXTERNAL file
syn keyword comalStatement       File FILE find Find FIND for For FOR
syn keyword comalStatement       func Func FUNC goto Goto GOTO handler
syn keyword comalStatement       Handler HANDLER if If IF import Import
syn keyword comalStatement       IMPORT in In IN input Input INPUT int
syn keyword comalStatement       Int INT label Label LABEL let Let LET
syn keyword comalStatement       list List LIST load Load LOAD loop Loop
syn keyword comalStatement       LOOP merge Merge MERGE  module Module
syn keyword comalStatement       MODULE new New NEW next Next NEXT null
syn keyword comalStatement       Null NULL of Of OF old Old OLD open Open
syn keyword comalStatement       OPEN  otherwise Otherwise OTHERWISE page
syn keyword comalStatement       Page PAGE print Print PRINT proc Proc
syn keyword comalStatement       PROC random Random RANDOM randomize
syn keyword comalStatement       Randomize RANDOMIZE read Read READ ref
syn keyword comalStatement       Ref REF rem Rem REM renum Renum RENUM
syn keyword comalStatement       repeat Repeat REPEAT report Report
syn keyword comalStatement       REPORT restore Restore RESTORE return
syn keyword comalStatement       Return RETURN rnd Rnd RND run Run RUN
syn keyword comalStatement       save Save SAVE scan Scan SCAN select
syn keyword comalStatement       Select SELECT  size Size SIZE   step
syn keyword comalStatement       Step STEP stop Stop STOP   then Then
syn keyword comalStatement       THEN  to To TO trap Trap TRAP  until
syn keyword comalStatement       Until UNTIL use Use USE using Using
syn keyword comalStatement       USING  when When WHEN while While WHILE
syn keyword comalStatement       write Write WRITE

syn keyword comalFunction        abs Abs ABS atn Atn ATN chr$ Chr$ CHR$
syn keyword comalFunction        cos Cos COS curcol Curcol CURCOL currow
syn keyword comalFunction        Currow CURROW eod Eod EOD eof Eof EOF
syn keyword comalFunction        eor Eor EOR err Err ERR errtext$
syn keyword comalFunction        Errtext$ ERRTEXT$ esc Esc ESC exp Exp
syn keyword comalFunction        EXP  freefile Freefile FREEFILE  get$
syn keyword comalFunction        Get$ GET$ int Int INT key$ Key$ KEY$ len
syn keyword comalFunction        Len LEN   log Log LOG ord Ord ORD   pi
syn keyword comalFunction        Pi PI random Random RANDOM Rnd RND run 
syn keyword comalFunction        sgn Sgn SGN sin Sin SIN spc$ Spc$ SPC$
syn keyword comalFunction        sqr Sqr SQR str$ Str$ STR$ tab Tab TAB
syn keyword comalFunction        tan Tan TAN  time Time TIME true True
syn keyword comalFunction        TRUE  val Val VAL  zone Zone ZONE 

syn keyword comalTodo contained	TODO

" Integer number, or floating point number without a dot.
syn match  comalNumber		"\<\d\+\>"

" Floating point number, with dot
syn match  comalNumber		"\<\d\+\.\d*\>"

" Floating point number, starting with a dot
syn match  comalNumber		"\.\d\+\>"

" Comment
syn region  comalComment      start="//" end="$" contains=comalTodo

" String and Character constants
syn match   comalSpecial contained "\\\d\d\d\|\\."
syn region  comalString		  start=+"+  skip=+\\\\\|\\"+  end=+"+  contains=comalSpecial

syn region  comalLineNumber	start="^\d" end="\s"
syn match   comalTypeSpecifier  "[a-zA-Z0-9\'][\$#]"ms=s+1

syn match   comalIdentifier  "[a-zA-Z][a-zA-Z0-9\']*[\$#]*"

syn match   comalMathsOperator   "-\|=\|[:<>+\*^\\]\|AND\|DIV\|MOD\|NOT\|OR\|XOR\|BITAND\|BITOR\|BITXOR"

" Define the default highlighting.
" Only when an item doesn't have highlighting yet

hi def link comalComment		Comment
hi def link comalLabel			Label
hi def link comalConditional	Conditional
hi def link comalRepeat			Repeat
hi def link comalNumber			Number
hi def link comalError			Error
hi def link comalStatement		Statement
hi def link comalString			String
hi def link comalMathsOperator  Operator
hi def link comalSpecial		Special
hi def link comalTodo			Todo
hi def link comalIdentifier     Identifier
hi def link comalFunction		Identifier
hi def link comalTypeSpecifier 	Type

let b:current_syntax = "comal"

let &cpo = s:cpo_save
unlet s:cpo_save
" vim: ts=8
