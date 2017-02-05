
!include "MUI.nsh"
!define MUI_FINISHPAGE_RUN "$INSTDIR\balancesheet.exe"
!define MUI_HEADERIMAGE
/*
!define MUI_HEADERIMAGE_BITMAP "balancesheet\dot3inst.bmp"
*/
/*
doesn't show?
!define MUI_HEADERIMAGE_UNBITMAP "balancesheet\dot3inst.bmp"
*/

Name "balancesheet"

OutFile "balancesheet_Setup.exe"

XPStyle on

InstallDir $PROGRAMFILES\balancesheet

InstallDirRegKey HKLM "Software\balancesheet" "Install_Dir"

SetCompressor /SOLID lzma
XPStyle on

Page components #"" ba ""
Page directory 
Page instfiles 
!insertmacro MUI_PAGE_FINISH

UninstPage uninstConfirm 
UninstPage instfiles 

!insertmacro MUI_LANGUAGE "English"

/*
AddBrandingImage top 65

Function ba
	File balancesheet\dot3.bmp
  SetBrandingImage balancesheet\dot3.bmp
FunctionEnd

Function un.ba
  SetBrandingImage balancesheet\dot3.bmp
FunctionEnd
*/

Function .onInit
  FindWindow $0 "balancesheet" ""
  StrCmp $0 0 continueInstall
    MessageBox MB_ICONSTOP|MB_OK "balancesheet is already running, please close it and try again."
    Abort
  continueInstall:
FunctionEnd

Function un.onInit
  FindWindow $0 "balancesheet" ""
  StrCmp $0 0 continueInstall
    MessageBox MB_ICONSTOP|MB_OK "balancesheet is still running, please close it and try again."
    Abort
  continueInstall:
FunctionEnd

Section "balancesheet (required)"

  SectionIn RO
  
  SetOutPath $INSTDIR
  
  File /r "PT\*.*"
  
  WriteRegStr HKLM SOFTWARE\balancesheet "Install_Dir" "$INSTDIR"
  
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\balancesheet" "DisplayName" "balancesheet"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\balancesheet" "UninstallString" '"$INSTDIR\uninstall.exe"'
  WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\balancesheet" "NoModify" 1
  WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\balancesheet" "NoRepair" 1
  WriteUninstaller "uninstall.exe"
  
SectionEnd

/*
Section "Visual C++ redistributable runtime"

  ExecWait '"$INSTDIR\redist\vcredist_x86.exe"'
  
SectionEnd
*/

Section "Start Menu Shortcuts"

  CreateDirectory "$SMPROGRAMS\balancesheet"
  CreateDirectory "$APPDATA\balancesheetdbs\"
  
  SetOutPath "$INSTDIR"
  
  CreateShortCut "$SMPROGRAMS\balancesheet\balancesheet.lnk" "$INSTDIR\balancesheet.exe" "" "$INSTDIR\balancesheet.exe" 0
  CreateShortCut "$SMPROGRAMS\balancesheet\Uninstall.lnk"        "$INSTDIR\uninstall.exe"        "" "$INSTDIR\uninstall.exe" 0
  CreateShortCut "$SMPROGRAMS\balancesheet\README.lnk"           "$INSTDIR\README.html"          "" "$INSTDIR\README.html" 0
  CreateShortCut "$SMPROGRAMS\balancesheet\Database Files.lnk"   "$APPDATA\balancesheetdbs\" "" "$APPDATA\balancesheetdbs\" 0
  
  CreateShortCut "$SMSTARTUP\balancesheet.lnk"                   "$INSTDIR\balancesheet.exe" "" "$INSTDIR\balancesheet.exe" 0
  
SectionEnd

Section "Uninstall"
  
  DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\balancesheet"
  DeleteRegKey HKLM SOFTWARE\balancesheet

  RMDir /r "$SMPROGRAMS\balancesheet"
  Delete "$SMSTARTUP\balancesheet.lnk"
  RMDir /r "$INSTDIR"

SectionEnd
