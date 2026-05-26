#!/usr/bin/env bash
set -euo pipefail

RID="${1:?Usage: build-native.sh <rid>}"
REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
NATIVE_SRC="$REPO_ROOT/native"
BUILD_DIR="$NATIVE_SRC/build/$RID"
RUNTIME_DIR="$REPO_ROOT/Nuklear.Net.Native/runtimes/$RID/native"

CMAKE_ARGS=()
LIB_NAME="libNuklearNetNative.so"

case "$RID" in
  linux-x64|linux-arm64)
    ;;
  osx-x64)
    LIB_NAME="libNuklearNetNative.dylib"
    CMAKE_ARGS+=(-DCMAKE_OSX_ARCHITECTURES=x86_64)
    ;;
  osx-arm64)
    LIB_NAME="libNuklearNetNative.dylib"
    CMAKE_ARGS+=(-DCMAKE_OSX_ARCHITECTURES=arm64)
    ;;
  win-x64|win-arm64)
    echo "Use build-native.ps1 on Windows for RID $RID" >&2
    exit 1
    ;;
  *)
    echo "Unknown RID: $RID" >&2
    exit 1
    ;;
esac

mkdir -p "$BUILD_DIR" "$RUNTIME_DIR"

cmake -S "$NATIVE_SRC" -B "$BUILD_DIR" "${CMAKE_ARGS[@]}"
cmake --build "$BUILD_DIR" --config Release

BUILT=""
for candidate in "$BUILD_DIR/$LIB_NAME" "$BUILD_DIR/Release/$LIB_NAME"; do
  if [[ -f "$candidate" ]]; then
    BUILT="$candidate"
    break
  fi
done

if [[ -z "$BUILT" ]]; then
  BUILT="$(find "$BUILD_DIR" -name "$LIB_NAME" -print -quit)"
fi

if [[ -z "$BUILT" || ! -f "$BUILT" ]]; then
  echo "Could not find $LIB_NAME under $BUILD_DIR" >&2
  exit 1
fi

cp "$BUILT" "$RUNTIME_DIR/$LIB_NAME"
echo "Installed $RUNTIME_DIR/$LIB_NAME"
