# This Makefile is to compile balancesheet with MinGW:
# * need set PATH to contain directories with MinGW/MSYS binaries
# * enter the "src" directory where this Makefile is
# * use "make" alone to make "pt.exe" (main executable)
# * use "make clean" to delete intermediate files

main: pt.exe

clean:
	rm -f *.o resource.h ../balancesheet/pt-resource.o

# source files used to build balancesheet executable pt.exe
# cpp: IdleTracker.cpp inputdlg.cpp balancesheet.cpp stdafx.cpp
# h: IdleTracker.h away.h day.h ddeutil.h inputdlg.h node.h
# h: nodedb.h prefview.h regview.h slaballoc.h statview.h stdafx.h
# h: timercallback.h tools.h win32tools.h ../balancesheet/resource.h
# rc: ../balancesheet/balancesheet.rc (and likely more there)
# lib: z psapi comctl32 comdlg32 shlwapi oleacc ole32 gdi32 stdc++

# need _WIN32_WINNT >= 0x0501, or ICC_STANDARD_CLASSES is undefined
COPT=-DWINVER=0x0501 -D_WIN32_WINNT=0x0501 -D_WIN32_IE=0x500 -DMINGW32_BUG

pt.exe: balancesheet.o IdleTracker.o inputdlg.o stdafx.o	\
	 ../balancesheet/pt-resource.o
	g++ balancesheet.o IdleTracker.o inputdlg.o stdafx.o	\
	 ../balancesheet/pt-resource.o  -o pt.exe			\
	 -lz -lpsapi -lcomctl32 -lcomdlg32 -lshlwapi -loleacc		\
	 -lole32 -lgdi32 -lstdc++ -Wl,--subsystem,windows

IdleTracker.o: Makefile IdleTracker.cpp
	g++ $(COPT) -Wno-write-strings -c IdleTracker.cpp

inputdlg.o: Makefile inputdlg.cpp inputdlg.h
	g++ $(COPT) -fpermissive -c inputdlg.cpp

balancesheet.o: Makefile balancesheet.cpp stdafx.h resource.h		\
	 IdleTracker.h win32tools.h inputdlg.h slaballoc.h tools.h day.h node.h	\
	 nodedb.h ddeutil.h timercallback.h statview.h prefview.h regview.h away.h
	g++ $(COPT) -Wno-multichar -Wno-write-strings -c balancesheet.cpp

stdafx.o: Makefile stdafx.cpp stdafx.h
	g++ $(COPT) -c stdafx.cpp

resource.h: ../balancesheet/resource.h
	cp -a ../balancesheet/resource.h ./

../balancesheet/pt-resource.o: ../balancesheet/balancesheet.rc	\
	../balancesheet/resource.h
	cd ..\\balancesheet; windres $(COPT) balancesheet.rc -o pt-resource.o
