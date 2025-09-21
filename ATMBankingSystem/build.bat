@echo off
REM =====================================================
REM ATM Banking System - Build Script
REM Builds all components using MSBuild
REM Requires: Visual Studio or Build Tools
REM =====================================================

setlocal

echo.
echo ========================================
echo    ATM Banking System - Build Script
echo ========================================
echo.

REM ---------------------------
REM Locate MSBuild
REM ---------------------------
set "MSBUILD_PATH="
set "VS2022=C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe"
set "VS2019=C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe"
set "BUILDTOOLS=C:\Program Files (x86)\Microsoft Visual Studio\2017\BuildTools\MSBuild\15.0\Bin\MSBuild.exe"
set "FRAMEWORK=C:\Program Files (x86)\MSBuild\14.0\Bin\MSBuild.exe"

if exist "%VS2022%" (
    set "MSBUILD_PATH=%VS2022%"
    echo Found MSBuild 2022: %MSBUILD_PATH%
) else if exist "%VS2019%" (
    set "MSBUILD_PATH=%VS2019%"
    echo Found MSBuild 2019: %MSBUILD_PATH%
) else if exist "%BUILDTOOLS%" (
    set "MSBUILD_PATH=%BUILDTOOLS%"
    echo Found Build Tools MSBuild: %MSBUILD_PATH%
) else if exist "%FRAMEWORK%" (
    set "MSBUILD_PATH=%FRAMEWORK%"
    echo Found .NET Framework MSBuild: %MSBUILD_PATH%
) else (
    echo ERROR: MSBuild not found!
    echo Please install Visual Studio or Build Tools and re-run this script.
    goto :error
)

echo.
echo Building solution...
echo ----------------------------------------

REM ---------------------------
REM Prepare output folder
REM ---------------------------
if not exist "bin" mkdir "bin"
REM Clean previous build outputs in bin (keeps bin itself)
echo Cleaning previous intermediate build folders...
if exist "bin\*" (
    rem optionally remove only files - here we clear bin content
    rmdir /s /q "bin" >nul 2>&1
    mkdir "bin"
)
REM remove common obj/bin folders in source tree
for /d %%D in (src\*\bin src\*\obj src\*\Debug src\*\Release) do (
    if exist "%%~D" (
        echo Removing %%~D
        rmdir /s /q "%%~D"
    )
)

REM ---------------------------
REM Helper to run msbuild and abort on error
REM ---------------------------
:run_msbuild
rem %1 = project path, %2 = optional extra msbuild properties
if "%~1"=="" (
    echo Internal error: no project passed to run_msbuild
    goto :error
)
echo.
echo Building project: %~1
"%MSBUILD_PATH%" "%~1" /p:Configuration=Release /p:Platform="Any CPU" %~2
if errorlevel 1 (
    echo ERROR: Build failed for %~1
    goto :error
)
goto :eof

REM ---------------------------
REM Build ActiveX Control
REM ---------------------------
call :run_msbuild "src\ActiveXControl\ActiveXControl.csproj" /p:OutputPath=..\..\bin\

REM ---------------------------
REM Build GRG Controls
REM ---------------------------
call :run_msbuild "src\GrgControls\GrgControls.csproj" /p:OutputPath=..\..\bin\

REM ---------------------------
REM Build Mock XFS
REM ---------------------------
call :run_msbuild "src\MockXFS\MockXFS.csproj" /p:OutputPath=..\..\bin\

REM ---------------------------
REM Build Host Simulator
REM ---------------------------
call :run_msbuild "src\HostSimulator\HostSimulator.csproj" /p:OutputPath=..\..\bin\

REM You can add further projects here following the same pattern:
REM call :run_msbuild "src\AnotherProject\AnotherProject.csproj" /p:OutputPath=..\..\bin\

REM ---------------------------
REM Copy UI files and resources
REM ---------------------------
echo.
echo Copying UI files and resources...
set "UI_TARGET=bin\UI"
if not exist "%UI_TARGET%" mkdir "%UI_TARGET%"

rem Check several possible UI source folders and copy if present
setlocal enabledelayedexpansion
set "POSSIBLE_UI_FOLDERS=src\UI src\WebUI src\WindowsUI src\WpfUI src\Angular\dist src\DesktopUI"

for %%F in (%POSSIBLE_UI_FOLDERS%) do (
    if exist "%%~F" (
        echo Copying files from %%~F to %UI_TARGET%
        xcopy "%%~F\*" "%UI_TARGET%\" /E /I /Y >nul
    )
)
endlocal

REM Copy common config files if present
if exist "src\app.config" copy /Y "src\app.config" "bin\" >nul
if exist "src\web.config" copy /Y "src\web.config" "bin\" >nul
if exist "src\HostSimulator\appsettings.json" copy /Y "src\HostSimulator\appsettings.json" "bin\" >nul

REM Copy any native/third-party DLLs from libs folder (if exists)
if exist "libs" (
    echo Copying libs\* to bin\
    xcopy "libs\*" "bin\" /S /I /Y >nul
)

REM Copy project outputs (if individual projects produced their own bin\Release)
echo Copying project outputs from project bin\Release (if any)...
for /d %%P in (src\*) do (
    if exist "%%P\bin\Release\*" (
        xcopy "%%P\bin\Release\*" "bin\" /S /I /Y >nul
    )
)

REM ---------------------------
REM Post-build steps - optional: generate a zip of bin
REM ---------------------------
set "ZIP_NAME=atm_build_%DATE:~10,4%-%DATE:~4,2%-%DATE:~7,2%.zip"
if exist "%ZIP_NAME%" del /Q "%ZIP_NAME%" >nul 2>&1
REM If 7zip is available, create an archive (optional)
if exist "%ProgramFiles%\7-Zip\7z.exe" (
    echo Creating ZIP archive %ZIP_NAME% ...
    "%ProgramFiles%\7-Zip\7z.exe" a -tzip "%ZIP_NAME%" "bin\*" >nul
) else (
    echo 7-Zip not found - skipping archive creation.
)

echo.
echo Build completed successfully.
echo Output available in: %CD%\bin
echo.

goto :end

:error
echo.
echo Build FAILED. See messages above.
endlocal
exit /b 1

:end
endlocal
exit /b 0
