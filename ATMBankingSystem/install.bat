@echo off
setlocal
REM ============================================================
REM ATM Banking System - INSTALL SCRIPT
REM Copies build artifacts and registers COM components
REM ============================================================

set "INSTALL_DIR=C:\ATMSystem"
set "BIN_DIR=%~dp0bin"

echo.
echo =========================================
echo Installing ATM Banking System
echo =========================================
echo.

if not exist "%BIN_DIR%" (
    echo ERROR: Build output not found in "%BIN_DIR%"
    echo Run build.bat first.
    goto :error
)

echo Creating install folder: %INSTALL_DIR%
if not exist "%INSTALL_DIR%" mkdir "%INSTALL_DIR%"

echo Copying files...
xcopy "%BIN_DIR%\*" "%INSTALL_DIR%\" /E /I /Y >nul
if errorlevel 1 goto :error

REM --- Register COM-visible assemblies (ActiveX / GrgDataPool) ---
echo.
echo Registering COM components...
set "FRAMEWORK_DIR=%SystemRoot%\Microsoft.NET\Framework\v4.0.30319"

REM Register ActiveX Control if exists
if exist "%INSTALL_DIR%\ActiveXControl.dll" (
    "%FRAMEWORK_DIR%\regasm.exe" "%INSTALL_DIR%\ActiveXControl.dll" /codebase /tlb
)

REM Register GrgControls.dll if needed
if exist "%INSTALL_DIR%\GrgControls.dll" (
    "%FRAMEWORK_DIR%\regasm.exe" "%INSTALL_DIR%\GrgControls.dll" /codebase /tlb
)

echo.
echo Installation complete!
echo Application installed to: %INSTALL_DIR%
echo.

goto :end

:error
echo.
echo INSTALL FAILED. Check paths and permissions.
endlocal
exit /b 1

:end
endlocal
exit /b 0
