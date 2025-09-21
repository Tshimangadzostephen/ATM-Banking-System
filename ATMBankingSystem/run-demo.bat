@echo off
setlocal
REM ============================================================
REM ATM Banking System - Demo Runner
REM Starts Host Simulator, Mock XFS and the HTA UI
REM ============================================================

set "INSTALL_DIR=C:\ATMSystem"
set "HTA_FILE=%INSTALL_DIR%\UI\index.hta"

echo.
echo =========================================
echo   ATM Banking System - Demo Runner
echo =========================================
echo.

REM --- Verify installation folder ---
if not exist "%INSTALL_DIR%" (
    echo ERROR: ATM Banking System is not installed.
    echo Run install.bat first.
    goto :end
)

REM --- Start Host Simulator ---
if exist "%INSTALL_DIR%\HostSimulator.exe" (
    echo Starting Host Simulator...
    start "Host Simulator" "%INSTALL_DIR%\HostSimulator.exe"
) else (
    echo WARNING: HostSimulator.exe not found in "%INSTALL_DIR%"
)

REM --- Start Mock XFS (if compiled as EXE) ---
if exist "%INSTALL_DIR%\MockXFS.exe" (
    echo Starting Mock XFS...
    start "Mock XFS" "%INSTALL_DIR%\MockXFS.exe"
) else (
    echo NOTE: Mock XFS executable not found (may be a DLL only)
)

REM --- Launch HTA front-end ---
if exist "%HTA_FILE%" (
    echo Opening ATM UI (HTA)...
    start "ATM Demo UI" mshta.exe "%HTA_FILE%"
) else (
    echo ERROR: HTA UI not found at "%HTA_FILE%"
)

echo.
echo ============================================================
echo Demo environment launched.
echo Close the windows to stop services when done.
echo ============================================================

:end
endlocal
exit /b 0
