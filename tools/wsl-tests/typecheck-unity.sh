#!/bin/bash
# Typechecks the game scripts (Scripts/**, Rules compiled inline) and the editor scripts (Editor/**) against Unity's reference assemblies.
# Needs game/Library/ScriptAssemblies, so Unity must have opened the project at least once. CS0649 warnings are expected.
source "$(dirname "${BASH_SOURCE[0]}")/common.sh"

SCRIPT_ASSEMBLIES="$GAME/Library/ScriptAssemblies"
BASE=$(refs "$UNITY_DATA/NetStandard/ref/2.1.0/netstandard.dll" "$UNITY_DATA"/Managed/UnityEngine/UnityEngine*.dll \
  "$SCRIPT_ASSEMBLIES/Unity.InputSystem.dll" "$SCRIPT_ASSEMBLIES/UnityEngine.UI.dll" "$SCRIPT_ASSEMBLIES/DungeonSurvivors.Pooling.dll")
EDITOR=$(refs "$UNITY_DATA"/Managed/UnityEditor*.dll "$UNITY_DATA"/Managed/UnityEngine/UnityEditor*.dll)

csc $BASE -t:library -out:"$(w "$OUT/Game.dll")" $(srcs "$GAME/Assets/_Project/Scripts" -not -path '*/Pooling/Core/*')
csc $BASE $EDITOR -r:"$(w "$OUT/Game.dll")" -t:library -out:"$(w "$OUT/GameEditor.dll")" $(srcs "$GAME/Assets/_Project/Editor")
echo "typecheck OK"
