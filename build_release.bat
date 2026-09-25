@echo off
setlocal
rem Builds release versions of MathExam into release\<platform>.
rem Runs the tests first, then publishes self-contained single-file builds for 64-bit
rem Windows (release\win-x64\MathExam.exe) and Linux (release\linux-x64\MathExam),
rem so the target PC does not need .NET installed.
rem Usage: build_release.bat [--no-pause]

cd /d "%~dp0"

where dotnet >nul 2>&1 || (
    echo ERROR: The .NET SDK was not found. Install it from https://dotnet.microsoft.com/download
    goto :fail
)

echo Running tests...
dotnet test MathExam.sln -c Release --nologo -v q || goto :fail

if exist release rmdir /s /q release || goto :fail
for %%R in (win-x64 linux-x64) do (
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
