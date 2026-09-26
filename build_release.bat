@echo off
setlocal
rem Builds release versions of MathExam into release\<platform>.
rem Runs the tests first, then publishes self-contained single-file builds for Windows
rem (release\win-x64\MathExam.exe) and Linux on x64, 64-bit ARM and 32-bit ARM, e.g. a Raspberry Pi
rem (release\linux-<arch>\MathExam), so the target computer does not need .NET installed.
rem Usage: build_release.bat [--no-pause]

cd /d "%~dp0"
set "RIDS=win-x64 linux-x64 linux-arm64 linux-arm"

where dotnet >nul 2>&1 || (
    echo ERROR: The .NET SDK was not found. Install it from https://dotnet.microsoft.com/download
    goto :fail
)

echo Running tests...
dotnet test MathExam.sln -c Release --nologo -v q || goto :fail

if exist release rmdir /s /q release || goto :fail
for %%R in (%RIDS%) do (
    echo.
    echo Publishing %%R...
    dotnet publish src\MathExam.App\MathExam.App.csproj -c Release -r %%R --self-contained true ^
        -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true ^
        -p:EnableCompressionInSingleFile=true -p:DebugType=none ^
        -o release\%%R --nologo -v q || goto :fail
)

echo.
echo Done:
echo   %~dp0release\win-x64\MathExam.exe
echo   %~dp0release\linux-x64\MathExam
echo   %~dp0release\linux-arm64\MathExam
echo   %~dp0release\linux-arm\MathExam
call :pause_unless %1
exit /b 0

:fail
echo.
echo BUILD FAILED.
call :pause_unless %1
exit /b 1

rem Keeps the window open when the script was double-clicked.
:pause_unless
if /i not "%~1"=="--no-pause" pause
exit /b 0
