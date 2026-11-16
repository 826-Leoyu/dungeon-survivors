# Shared setup for the WSL fallback scripts: compiles with the Roslyn and .NET runtime bundled with the Unity Editor on Windows.
# Override UNITY_DATA_WIN if Unity is installed elsewhere, and OUT to change where build output goes (never inside the repo).
set -euo pipefail

UNITY_DATA_WIN=${UNITY_DATA_WIN:-'D:\Development\Unity\6000.6.8f1\Editor\Data'}
UNITY_DATA=$(wslpath -u "$UNITY_DATA_WIN")
HERE=$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)
GAME=$(cd "$HERE/../../game" && pwd)
OUT=${OUT:-/tmp/dungeon-survivors-tests}

DOTNET="$UNITY_DATA/NetCoreRuntime/dotnet.exe"
CSC_DLL=$(wslpath -w "$(ls -d "$UNITY_DATA"/DotNetSdk/sdk/*/Roslyn/bincore/csc.dll | head -n 1)")
NETCORE_DIR=$(ls -d "$UNITY_DATA"/NetCoreRuntime/shared/Microsoft.NETCore.App/* | head -n 1)
NETCORE_VERSION=$(basename "$NETCORE_DIR")

mkdir -p "$OUT"

w() { wslpath -w "$1"; }

# Prints -r: options for the given DLLs, as Windows paths.
refs() { for f in "$@"; do printf -- '-r:%s ' "$(w "$f")"; done; }

# Prints the .cs files under the given find arguments, as Windows paths.
srcs() { find "$@" -name '*.cs' | while read -r f; do printf '%s ' "$(w "$f")"; done; }

csc() { "$DOTNET" "$CSC_DLL" -nologo -nostdlib -noconfig -langversion:9 -nowarn:1701,1702 "$@"; }
