@echo off
setlocal
rem Starts the Windows release build of MathExam, building it first if it does not exist yet.

cd /d "%~dp0"
set "EXE=release\win-x64\MathExam.exe"

if not exist "%EXE%" (
    echo No release build found - building it first...
    call "%~dp0build_release.bat" --no-pause || (
        pause
        exit /b 1
    )
)

start "" "%EXE%"
