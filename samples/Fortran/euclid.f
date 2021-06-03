      PROGRAM EUCLID
      WRITE(UNIT=*, FMT=*) 'Enter two integers'
      READ(UNIT=*, FMT=*) M, N
10    IF(M .NE. N) THEN 
          IF(M .GT. N) THEN
              M=M-N
          ELSE
              N=N-M
          END IF
          GO TO 10
      END IF
      WRITE(UNIT=*, FMT=*)'Highest common factor = ', M
      END