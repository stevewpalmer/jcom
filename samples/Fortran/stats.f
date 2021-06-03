      PROGRAM STATS
      CHARACTER FNAME*50
      REAL X(1000)
      WRITE(UNIT=*, FMT=*) 'Enter data file name:' 
      READ(UNIT=*, FMT='(A)') FNAME
      OPEN(UNIT=1, FILE=FNAME, STATUS='OLD')
*Read number of data points NPTS 
      
      READ(UNIT=1, FMT=*) NPTS
      WRITE(UNIT=*, FMT=*) NPTS, ' data points'
      IF(NPTS .GT. 1000) STOP 'Too many data points'
      READ(UNIT=1, FMT=*) (X(I), I = 1,NPTS)
      CALL MEANSD(X, NPTS, AVG, SD)
      WRITE(UNIT=*, FMT=*) 'Mean =', AVG, ' Std Deviation =', SD
      END

      SUBROUTINE MEANSD(X, NPTS, AVG, SD)
      INTEGER NPTS
      REAL X(NPTS), AVG, SD
      SUM = 0.0
      DO 15, I = 1,NPTS
      SUM = SUM + X(I)
15    CONTINUE
      AVG = SUM / NPTS
      SUMSQ = 0.0
      DO 25, I = 1,NPTS
      SUMSQ = SUMSQ + (X(I) - AVG)**2
25    CONTINUE
      SD = SQRT(SUMSQ /(NPTS-1))
      END
      