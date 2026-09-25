@echo off
setlocal
rem Starts the release build of MathExam, building it first if it does not exist yet.

cd /d "%~dp0"
set "EXE=release\MathExam.App.exe"

if not exist "%EXE%" (
    echo No release build found - building it first...
    call build_release.bat --no-pause || (
        pause
        exit /b 1
    )
)

start "" "%EXE%"
