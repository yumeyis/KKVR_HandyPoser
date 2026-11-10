#!/usr/bin/env bash
# Copies the Koikatsu official-VR assemblies needed for compilation into lib/.
#
# Usage:
#   tools/copy-game-libs.sh "/path/to/Koikatsu Party"
#   (e.g. ~/.steam/steam/steamapps/common/Koikatsu Party, or /mnt/c/... under WSL)
set -euo pipefail

if [[ $# -ne 1 ]]; then
    echo "usage: $0 <game dir>" >&2
    exit 1
fi

game_dir="$1"
managed=""
for candidate in "$game_dir/Koikatsu Party VR_Data/Managed" "$game_dir/KoikatuVR_Data/Managed"; do
    if [[ -d "$candidate" ]]; then
        managed="$candidate"
        break
    fi
done

if [[ -z "$managed" ]]; then
    echo "error: no VR Managed folder found under '$game_dir'" >&2
    echo "expected 'Koikatsu Party VR_Data/Managed' or 'KoikatuVR_Data/Managed'" >&2
    exit 1
fi

lib_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)/lib"
mkdir -p "$lib_dir"

for f in Assembly-CSharp.dll Assembly-CSharp-firstpass.dll UnityEngine.dll UnityEngine.UI.dll; do
    cp -f "$managed/$f" "$lib_dir/"
    echo "copied $f"
done
echo "Done. Assemblies copied from $managed"
