      PROGRAM DATE
      INTEGER YEAR, MONTH,DAYS
      CHARACTER*10 MNAMES(12)

      DATA MNAMES/'JANUARY','FEBRUARY','MARCH',
     1 'APRIL','MAY','JUNE','JULY','AUGUST', 'SEPTEMBER',
     2 'OCTOBER', 'NOVEMBER', 'DECEMBER'/

      WRITE (*,*) 'Enter a year and month'
      READ (*,*) YEAR, MONTH
      CALL CALEND(YEAR,MONTH,DAYS)
      WRITE (*,*) 'There are',DAYS,'days in',MNAMES(MONTH),'in',YEAR
      END

      SUBROUTINE CALEND(YEAR, MONTH, DAYS)
      INTEGER YEAR, MONTH, DAYS
      GO TO(310,280,310,300,310,300,310,310,300,310,300,310)MONTH
*           Jan Feb Mar Apr May Jun Jly Aug Sep Oct Nov Dec
      STOP 'Impossible month number'
*February: has 29 days in leap year, 28 otherwise.
280   IF(MOD(YEAR,400) .EQ. 0 .OR. (MOD(YEAR,100) .NE. 0
     $       .AND. MOD(YEAR,4) .EQ. 0)) THEN
          DAYS = 29
      ELSE
          DAYS = 28
      END IF
      GO TO 1000
*   Short months
300   DAYS = 30      
      GO TO 1000
*   Long months
310   DAYS = 31
* return the value of DAYS 
1000  END
