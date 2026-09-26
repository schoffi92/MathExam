@echo off
setlocal
rem Creates release\MathExam-linux-<arch>.tar.gz for x64, arm64 and arm (32-bit): the self-contained
rem Linux build plus a README and the license. Builds first (build_release.bat), so the archives
rem always match the current code.
rem Usage: package_linux.bat [--no-pause]

cd /d "%~dp0"

where tar >nul 2>&1 || (
    echo ERROR: tar.exe was not found. It is included in Windows 10 version 1803 and later.
    goto :fail
)

call "%~dp0build_release.bat" --no-pause || goto :fail

echo.
echo Packaging...
for %%R in (linux-x64 linux-arm64 linux-arm) do (
    call :package %%R || goto :fail
)

echo.
echo Done.
call :pause_unless %1
exit /b 0

:fail
echo.
echo PACKAGING FAILED.
call :pause_unless %1
exit /b 1

rem Packs release\<rid> into release\MathExam-<rid>.tar.gz.
:package
copy /y packaging\linux\README.txt release\%1\README.txt >nul || exit /b 1
copy /y license.md release\%1\LICENSE.md >nul || exit /b 1
rem The mtree file list sets Linux file permissions (the program must be executable),
rem which tar cannot take from the Windows file system. Its paths are relative to release\<rid>.
pushd release\%1
tar -czf ..\MathExam-%1.tar.gz --uname root --gname root @..\..\packaging\linux\files.mtree
set "TAR_RESULT=%errorlevel%"
popd
if not "%TAR_RESULT%"=="0" exit /b 1
echo   %~dp0release\MathExam-%1.tar.gz
exit /b 0

rem Keeps the window open when the script was double-clicked.
:pause_unless
if /i not "%~1"=="--no-pause" pause
exit /b 0
