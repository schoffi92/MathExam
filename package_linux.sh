#!/usr/bin/env bash
# Creates release/MathExam-linux-<arch>.tar.gz for x64, arm64 and arm (32-bit): the self-contained
# Linux build plus a README and the license. Builds first (build_release.sh), so the archives
# always match the current code.
set -euo pipefail
cd "$(dirname "$0")"

bash ./build_release.sh

echo
echo "Packaging..."
for rid in linux-x64 linux-arm64 linux-arm; do
    stage=release/package/MathExam
    rm -rf release/package
    mkdir -p "$stage"
    install -m 755 "release/$rid/MathExam" "$stage/MathExam"
    install -m 644 packaging/linux/README.txt "$stage/README.txt"
    install -m 644 license.md "$stage/LICENSE.md"
    tar -czf "release/MathExam-$rid.tar.gz" --owner=0 --group=0 -C release/package MathExam
    rm -rf release/package
    echo "  $PWD/release/MathExam-$rid.tar.gz"
done

echo
echo "Done."
