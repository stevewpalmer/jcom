      program ninetyninebottles
      integer bottles

*     99 Bottles of Beer, as implemented in FORTRAN 77
*     Written by Alex Ford - gustavderdrache@bellsouth.net
*     Notable feature: Arithmetic IF statement

      bottles = 99

*     Format statements
 1    format (I2, A)
 2    format (A)
 3    format (I2, A, /)
 4    format (A, /)

*     First 98 or so verses
 10   write (*,1) bottles, ' bottles of beer on the wall,'
      write (*,1) bottles, ' bottles of beer.'
      write (*,2) 'Take one down, pass it around...'
      if (bottles - 1 .gt. 1) then
         write (*,3) bottles - 1, ' bottles of beer on the wall.'
      else
         write (*,3) bottles - 1, ' bottle of beer on the wall.'
      end if

      bottles = bottles - 1

      if (bottles - 1) 30, 20, 10

*     Last verse
 20   write (*,1) bottles, ' bottle of beer on the wall,'
      write (*,1) bottles, ' bottle of beer.'
      write (*,2) 'Take one down, pass it around...'
      write (*,4) 'No bottles of beer on the wall.'

 30   stop
      end