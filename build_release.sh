#!/usr/bin/env bash
# Builds release versions of MathExam into release/<platform>.
# Runs the tests first, then publishes self-contained single-file builds for Windows
# (release/win-x64/MathExam.exe) and Linux on x64, 64-bit ARM and 32-bit ARM, e.g. a Raspberry Pi
# (release/linux-<arch>/MathExam), so the target computer does not need .NET installed.
set -euo pipefail
cd "$(dirname "$0")"

rids="win-x64 linux-x64 linux-arm64 linux-arm"

if ! command -v dotnet >/dev/null; then
    echo "ERROR: The .NET SDK was not found. Install it from https://dotnet.microsoft.com/download" >&2
    exit 1
fi

echo "Running tests..."
dotnet test MathExam.sln -c Release --nologo -v q

rm -rf release
for rid in $rids; do
    echo
    echo "Publishing $rid..."
    dotnet publish src/MathExam.App/MathExam.App.csproj -c Release -r "$rid" --self-contained true \
        -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true \
        -p:EnableCompressionInSingleFile=true -p:DebugType=none \
        -o "release/$rid" --nologo -v q
done

echo
echo "Done:"
for rid in $rids; do
    ls -d "$PWD/release/$rid/"MathExam*
done
