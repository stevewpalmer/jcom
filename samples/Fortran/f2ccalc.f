      REAL C,TEMP
      C(F) = 5.0*(F - 32.0)/9.0
      WRITE (*,*) 'Enter temperature in Fahrenheit'
      READ (*,*,ERR=90,END=91) TEMP
      PRINT *,'Fahrenheit',TEMP,' is Celsius',C(TEMP)
      STOP
90    PRINT *,'Input error'
      STOP
91    PRINT *,'Nothing entered'
      STOP
      END
      