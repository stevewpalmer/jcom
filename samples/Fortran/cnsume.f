      PROGRAM CNSUME
      CHARACTER * 50 BUF
      INTEGER NEXT
      CHARACTER C, PRDUCE
      DATA NEXT /1/, BUF /' '/
6     C = PRDUCE()
      BUF(NEXT:NEXT) = C
      NEXT = NEXT + 1
      IF (C .NE. ' ') GOTO 6
      WRITE (*,*) BUF
      END

      CHARACTER FUNCTION PRDUCE()
      CHARACTER * 80 BUFFER
      INTEGER NEXT
      SAVE BUFFER, NEXT
      DATA NEXT /81/
      IF (NEXT .GT. 80) THEN
      READ (*,'(A)') BUFFER
      NEXT = 1
      END IF
      PRDUCE = BUFFER(NEXT:NEXT)
      NEXT = NEXT + 1
      END
