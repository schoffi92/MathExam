@echo off
setlocal
rem Creates release\MathExam-linux-x64.tar.gz: the self-contained Linux build plus a README.
rem Builds first (build_release.bat), so the archive always matches the current code.
rem Usage: package_linux.bat [--no-pause]

cd /d "%~dp0"

where tar >nul 2>&1 || (
    echo ERROR: tar.exe was not found. It is included in Windows 10 version 1803 and later.
    goto :fail
)

call "%~dp0build_release.bat" --no-pause || goto :fail

echo.
echo Packaging...
copy /y packaging\linux\README.txt release\linux-x64\README.txt >nul || goto :fail
rem The mtree file list sets Linux file permissions (the program must be executable),
rem which tar cannot take from the Windows file system.
pushd release
tar -czf MathExam-linux-x64.tar.gz --uname root --gname root @..\packaging\linux\files.mtree
set "TAR_RESULT=%errorlevel%"
popd
if not "%TAR_RESULT%"=="0" goto :fail

echo.
echo Done: %~dp0release\MathExam-linux-x64.tar.gz
call :pause_unless %1
exit /b 0

:fail
echo.
echo PACKAGING FAILED.
call :pause_unless %1
exit /b 1

rem Keeps the window open when the script was double-clicked.
:pause_unless
if /i not "%~1"=="--no-pause" pause
exit /b 0
