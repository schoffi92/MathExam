#!/usr/bin/env bash
# Starts the Linux release build of MathExam for this computer's processor (x64, 64-bit ARM
# or 32-bit ARM), building it first if it does not exist yet.
set -euo pipefail
cd "$(dirname "$0")"

case "$(uname -m)" in
    x86_64) rid=linux-x64 ;;
    aarch64 | arm64) rid=linux-arm64 ;;
    armv7l | armv8l) rid=linux-arm ;;
    *)
        echo "ERROR: MathExam has no build for the $(uname -m) processor." >&2
        exit 1
        ;;
esac

exe=release/$rid/MathExam
if [ ! -x "$exe" ]; then
    echo "No release build found - building it first..."
    bash ./build_release.sh
fi

exec "$exe" "$@"
