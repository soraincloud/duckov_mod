#!/usr/bin/env bash
set -e

MOD_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
MOD_NAME="GoldenApple"

if [[ -z "${DUCKOV_PATH:-}" ]]; then
  echo "DUCKOV_PATH is not set. Example:" >&2
  echo "  export DUCKOV_PATH=\"/Volumes/Kingston-1TB/SteamLibrary/steamapps/common/Escape from Duckov\"" >&2
  exit 1
fi

DEST_MAC="$DUCKOV_PATH/Duckov.app/Contents/Mods/$MOD_NAME"
DEST_WIN="$DUCKOV_PATH/Duckov_Data/Mods/$MOD_NAME"

if [[ -d "$DUCKOV_PATH/Duckov.app" ]]; then
  DEST="$DEST_MAC"
else
  DEST="$DEST_WIN"
fi

mkdir -p "$DEST"

(dotnet build "$MOD_DIR/$MOD_NAME.csproj" -c Release -v minimal)
cp -f "$MOD_DIR/bin/Release/netstandard2.1/$MOD_NAME.dll" "$MOD_DIR/$MOD_NAME.dll"

src_info="$MOD_DIR/info.ini"
dest_info="$DEST/info.ini"

get_pfid() {
  local f="$1"
  if [[ -f "$f" ]]; then
    local v
    v=$(grep -E "^publishedFileId[[:space:]]*=" "$f" | head -n 1 | sed -E "s/.*=[[:space:]]*([0-9]+).*/\1/") || true
    if [[ "$v" =~ ^[0-9]+$ ]] && [[ "$v" != "0" ]]; then
      echo "$v"
      return 0
    fi
  fi
  return 1
}

PFID=""
if PFID=$(get_pfid "$dest_info"); then
  :
elif PFID=$(get_pfid "$src_info"); then
  :
else
  PFID="0"
fi

rsync -av --delete \
  --exclude "bin/" \
  --exclude "obj/" \
  --exclude "*.cs" \
  --exclude "*.csproj" \
  --exclude "README.md" \
  --exclude "STEAM_DESCRIPTION.md" \
  --exclude "info.ini" \
  "$MOD_DIR/" \
  "$DEST/"

if grep -qE "^publishedFileId[[:space:]]*=" "$src_info"; then
  sed -E "s/^publishedFileId[[:space:]]*=.*/publishedFileId = $PFID/" "$src_info" > "$dest_info"
else
  cat "$src_info" > "$dest_info"
  echo "" >> "$dest_info"
  echo "publishedFileId = $PFID" >> "$dest_info"
fi

echo "Deployed to: $DEST"
echo "publishedFileId = $PFID"