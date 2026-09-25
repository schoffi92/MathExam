#!/usr/bin/env bash
# Starts the Linux release build of MathExam, building it first if it does not exist yet.
set -euo pipefail
cd "$(dirname "$0")"

exe=release/linux-x64/MathExam
if [ ! -x "$exe" ]; then
    echo "No release build found - building it first..."
    bash ./build_release.sh
fi

exec "$exe" "$@"
