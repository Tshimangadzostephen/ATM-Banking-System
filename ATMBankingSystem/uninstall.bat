@echo off
setlocal
REM ============================================================
REM ATM Banking System - UNINSTALL SCRIPT
REM Unregisters COM components and removes install folder
REM ============================================================

set "INSTALL_DIR=C:\ATMSystem"
set "FRAMEWORK_DIR=%SystemRoot%\Microsoft.NET\Framework\v4.0.30319"

echo.
echo =========================================
echo Uninstalling ATM Banking System
echo =========================================
echo.

REM --- Unregister COM components ---
if exist "%INSTALL_DIR%\ActiveXControl.dll" (
    echo Unregistering ActiveXControl.dll ...
    "%FRAMEWORK_DIR%\regasm.exe" /unregister "%INSTALL_DIR%\ActiveXControl.dll"
)
if exist "%INSTALL_DIR%\GrgControls.dll" (
    echo Unregistering GrgControls.dll ...
    "%FRAMEWORK_DIR%\regasm.exe" /unregister "%INSTALL_DIR%\GrgControls.dll"
)

REM --- Remove installed files ---
if exist "%INSTALL_DIR%" (
    echo Removing %INSTALL_DIR% ...
    rmdir /s /q "%INSTALL_DIR%"
)

echo.
echo Uninstallation complete.
echo.

endlocal
exit /b 0
