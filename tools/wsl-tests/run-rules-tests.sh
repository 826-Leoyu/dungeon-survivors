#!/bin/bash
# Usage: run-rules-tests.sh [TestClassName]
# Compiles the rules assembly (Scripts/Rules) and its EditMode tests outside Unity, then runs them with Runner.cs.
# Runner.cs handles [Test] and [TestCase] only; the Pooling tests are not compiled or run.
source "$(dirname "${BASH_SOURCE[0]}")/common.sh"

cp "$(ls "$GAME"/Library/PackageCache/com.unity.ext.nunit@*/net472/unity-custom/nunit.framework.dll | head -n 1)" "$OUT/"

NETCORE_REFS=$(refs $(ls "$NETCORE_DIR"/*.dll | grep -E '/(System|Microsoft|netstandard|mscorlib)[^/]*\.dll$' | grep -v -E 'Native|Microsoft.DiaSymReader|Microsoft.VisualBasic.dll$'))
NUNIT=$(refs "$OUT/nunit.framework.dll")

csc $NETCORE_REFS -t:library -out:"$(w "$OUT/Rules.dll")" $(srcs "$GAME/Assets/_Project/Scripts/Rules" -maxdepth 1)
csc $NETCORE_REFS $NUNIT -r:"$(w "$OUT/Rules.dll")" -t:library -out:"$(w "$OUT/Tests.dll")" $(srcs "$GAME/Assets/_Project/Tests/EditMode" -maxdepth 1)
csc $NETCORE_REFS $NUNIT -t:exe -out:"$(w "$OUT/Runner.dll")" "$(w "$HERE/Runner.cs")"

cat > "$OUT/Runner.runtimeconfig.json" <<JSON
{"runtimeOptions":{"tfm":"net8.0","framework":{"name":"Microsoft.NETCore.App","version":"$NETCORE_VERSION"}}}
JSON

"$DOTNET" "$(w "$OUT/Runner.dll")" "$(w "$OUT/Tests.dll")" "$@"
