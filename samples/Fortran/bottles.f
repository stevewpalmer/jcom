      program ninetyninebottles
      integer btls

*     99 Bottles of Beer, as implemented in FORTRAN 77
*     Written by Alex Ford - gustavderdrache@bellsouth.net
*     Notable feature: Arithmetic IF statement

      btls = 99

*     Format statements
 1    format (I2, A)
 2    format (A)
 3    format (I2, A, /)
 4    format (A, /)

*     First 98 or so verses
 10   write (*,1) btls, ' bottles of beer on the wall,'
      write (*,1) btls, ' bottles of beer.'
      write (*,2) 'Take one down, pass it around...'
      if (btls - 1 .gt. 1) then
         write (*,3) btls - 1, ' bottles of beer on the wall.'
      else
         write (*,3) btls - 1, ' bottle of beer on the wall.'
      end if

      btls = btls - 1

      if (btls - 1) 30, 20, 10

*     Last verse
 20   write (*,1) btls, ' bottle of beer on the wall,'
      write (*,1) btls, ' bottle of beer.'
      write (*,2) 'Take one down, pass it around...'
      write (*,4) 'No bottles of beer on the wall.'

 30   stop
      end