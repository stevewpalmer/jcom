      PROGRAM IMPDATA
      DIMENSION JSPKT(16)
      DATA(JSPKT(I),I=1,16)/24,29,0,31,0,31,38,38,42,42,43,46,77,71
     1 ,73,75/
      DO 100,I=1,16
          PRINT *,JSPKT(I)
100   CONTINUE
      END