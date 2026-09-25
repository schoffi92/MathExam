@echo off
setlocal
rem Builds a release of MathExam into the "release" folder.
rem Runs the tests first, then publishes a self-contained single-file exe for
rem 64-bit Windows, so the target PC does not need .NET installed.
rem Usage: build_release.bat [--no-pause]

cd /d "%~dp0"

where dotnet >nul 2>&1 || (
    echo ERROR: The .NET SDK was not found. Install it from https://dotnet.microsoft.com/download
    goto :fail
)

echo Running tests...
dotnet test MathExam.sln -c Release --nologo -v q || goto :fail

echo.
echo Publishing...
if exist release rmdir /s /q release || goto :fail
dotnet publish src\MathExam.App\MathExam.App.csproj -c Release -r win-x64 --self-contained true ^
    -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true ^
    -p:EnableCompressionInSingleFile=true -p:DebugType=none ^
    -o release --nologo -v q || goto :fail

echo.
echo Done: %~dp0release\MathExam.App.exe
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
