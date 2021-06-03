      PROGRAM VOWELS
      CHARACTER*80 INPUT
      WRITE (*,*) 'Enter some text'
      READ (*,'A') INPUT
      WRITE (*,*) 'There are',VOWELS(INPUT),'vowels in the text'
      END

      INTEGER FUNCTION VOWELS(STRING)
      CHARACTER*(*) STRING
      VOWELS = 0
      DO 25, K = 1,LEN(STRING)
          IF( INDEX('AEIOU', STRING(K:K)) .NE. 0) THEN
              VOWELS = VOWELS + 1
          END IF
25    CONTINUE
      END