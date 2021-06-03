      PROGRAM TRIANG
      WRITE(UNIT=*,FMT=*)'Enter lengths of three sides:' 
      READ(UNIT=*,FMT=*) SIDEA, SIDEB, SIDEC 
      WRITE(UNIT=*,FMT=*)'Area is ', AREA3(SIDEA,SIDEB,SIDEC) 
      END

      REAL FUNCTION AREA3(A, B, C)
*Computes the area of a triangle from lengths of its sides.
*If arguments are invalid issues error message and returns zero.
      REAL A, B, C
      S = (A + B + C)/2.0
      FACTOR = S * (S-A) * (S-B) * (S-C)
      IF(FACTOR .LE. 0.0) THEN
        STOP 'Impossible triangle'
      ELSE
        AREA3 = SQRT(FACTOR)
      END IF
      END