# This file is included by both the Unix makefile and the Windows
# makefile.mak.

ALLTESTS=FM002.exe FM003.exe FM004.exe FM005.exe FM006.exe  \
    FM007.exe FM008.exe FM009.exe FM012.exe FM014.exe FM016.exe \
    FM017.exe FM018.exe FM019.exe FM020.exe FM021.exe FM025.exe \
    FM026.exe FM028.exe FM030.exe FM031.exe FM032.exe FM033.exe \
    FM034.exe FM035.exe FM036.exe FM037.exe FM038.exe FM039.exe \
    FM040.exe FM041.exe FM042.exe FM043.exe FM044.exe FM045.exe \
    FM050.exe FM056.exe FM060.exe FM061.exe FM062.exe FM080.exe \
    FM097.exe FM098.exe FM099.exe FM100.exe FM101.exe FM102.exe \
    FM103.exe FM104.exe FM105.exe FM106.exe FM107.exe FM108.exe \
    FM201.exe FM202.exe FM203.exe FM204.exe FM205.exe FM251.exe \
    FM253.exe FM254.exe FM255.exe FM256.exe FM301.exe FM306.exe \
    FM307.exe FM311.exe FM351.exe FM352.exe FM354.exe FM355.exe \
    FM356.exe FM357.exe FM359.exe FM360.exe FM361.exe FM362.exe \
    FM363.exe FM364.exe FM368.exe FM369.exe FM370.exe FM371.exe \
    FM372.exe FM373.exe FM374.exe FM375.exe FM376.exe FM377.exe \
    FM378.exe FM379.exe FM401.exe FM402.exe FM405.exe FM406.exe \
    FM407.exe FM411.exe FM413.exe FM514.exe FM520.exe FM701.exe \
    FM711.exe FM715.exe FM718.exe FM800.exe FM801.exe FM802.exe \
    FM803.exe FM804.exe FM805.exe FM806.exe FM807.exe FM808.exe \
    FM810.exe FM812.exe FM814.exe FM816.exe FM818.exe FM819.exe \
    FM821.exe FM822.exe FM823.exe FM824.exe FM825.exe FM826.exe \
    FM827.exe FM832.exe FM910.exe FM912.exe FM914.exe FM915.exe \
    FM916.exe FM917.exe FM919.exe FM920.exe FM921.exe FM922.exe 

# Non-interactive tests that accept data file stdin where the data
# file basename is the same as the test executable name.
ALLDATATESTS=FM923.exe

# Tests that require interaction when run
INTTESTS=FM001.exe FM109.exe FM257.exe FM258.exe FM259.exe \
    FM260.exe FM261.exe FM353.exe FM905.exe FM907.exe

# Interactive tests that accept data file stdin where the data file
# basename is the same as the test executable name.
INTDATATESTS=FM110.exe FM403.exe FM404.exe FM900.exe FM901.exe

# These tests compile but crash in the .NET runtime.
BUGTESTS=FM317.exe FM328.exe

# All of these fail to compile cleanly due to unfinished implementation
# Once support is added, gradually move these to the appropriate
# successful tests groups.
FAILTESTS=FM010.exe FM011.exe FM013.exe FM022.exe FM023.exe \
    FM024.exe FM111.exe FM200.exe FM252.exe FM300.exe FM302.exe \
    FM308.exe FM500.exe FM503.exe FM506.exe FM509.exe FM517.exe \
    FM700.exe FM710.exe FM719.exe FM722.exe FM809.exe FM811.exe \
    FM813.exe FM815.exe FM817.exe FM820.exe FM828.exe FM829.exe \
    FM830.exe FM831.exe FM833.exe FM834.exe FM903.exe FM906.exe \
    FM908.exe FM909.exe
