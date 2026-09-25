#!/usr/bin/env bash
# Builds release versions of MathExam into release/<platform>.
# Runs the tests first, then publishes self-contained single-file builds for 64-bit Linux
# (release/linux-x64/MathExam) and Windows (release/win-x64/MathExam.exe), so the target
# PC does not need .NET installed.
set -euo pipefail
cd "$(dirname "$0")"

if ! command -v dotnet >/dev/null; then
    echo "ERROR: The .NET SDK was not found. Install it from https://dotnet.microsoft.com/download" >&2
    exit 1
fi

echo "Running tests..."
dotnet test MathExam.sln -c Release --nologo -v q

rm -rf release
for rid in linux-x64 win-x64; do
    echo
    echo "Publishing $rid..."
    dotnet publish src/MathExam.App/MathExam.App.csproj -c Release -r "$rid" --self-contained true \
        -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true \
        -p:EnableCompressionInSingleFile=true -p:DebugType=none \
        -o "release/$rid" --nologo -v q
done

echo
echo "Done:"
echo "  $PWD/release/linux-x64/MathExam"
echo "  $PWD/release/win-x64/MathExam.exe"
