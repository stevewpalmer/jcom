      PROGRAM INTR
      WRITE(UNIT=*,FMT=*)'Result is ', INTRTEST() 
      END

      DOUBLE PRECISION FUNCTION INTRTEST
      INTRINSIC DSIN
      INTRTEST=CALCIT(DSIN,23.0D0)
      END                          

      FUNCTION CALCIT(F,Y)
      DOUBLE PRECISION F,Y,CALCIT
      EXTERNAL F
      CALCIT=F(Y)
      END

