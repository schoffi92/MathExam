#!/usr/bin/env bash
# Creates release/MathExam-linux-x64.tar.gz: the self-contained Linux build plus a README.
# Builds first (build_release.sh), so the archive always matches the current code.
set -euo pipefail
cd "$(dirname "$0")"

bash ./build_release.sh

echo
echo "Packaging..."
stage=release/package/MathExam
rm -rf release/package
mkdir -p "$stage"
install -m 755 release/linux-x64/MathExam "$stage/MathExam"
install -m 644 packaging/linux/README.txt "$stage/README.txt"
install -m 644 license.md "$stage/LICENSE.md"
tar -czf release/MathExam-linux-x64.tar.gz --owner=0 --group=0 -C release/package MathExam
rm -rf release/package

echo
echo "Done: $PWD/release/MathExam-linux-x64.tar.gz"
