      PROGRAM CURVES
      INTRINSIC SIN, TAN
      EXTERNAL MESSY
      CALL GRAPH(SIN, 0.0, 3.14159) 
      CALL GRAPH(TAN, 0.0, 0.5)
      CALL GRAPH(MESSY, 0.1, 0.9)
      END

      SUBROUTINE GRAPH(MYFUNC, XMIN, XMAX)
*Plots functional form of MYFUNC(X) with X in range XMIN:XMAX.
      REAL MYFUNC, XMIN, XMAX
      XDELTA = (XMAX - XMIN) / 100.0 
      DO 25, I = 0,100
         X = XMIN + I * XDELTA
         Y = MYFUNC(X)
         CALL PLOT(X, Y)
25    CONTINUE 
      END

      REAL FUNCTION MESSY(X)
      MESSY = COS(0.1*X) + 0.02 * SIN(SQRT(X))
      END

      SUBROUTINE PLOT(X,Y)
      WRITE (*,*) X,Y
      END
    
