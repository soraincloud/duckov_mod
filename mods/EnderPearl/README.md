# 末影珍珠（EnderPearl）

功能：
- 新增物品「末影珍珠」：可使用（消耗 1 个）并投掷出一个投掷物
- 投掷物落地（首次碰撞）时，将角色传送到落点
- 加入 NPC 商店「橘子」（装备商人 `Merchant_Equipment`）售价 `$1`

## 构建

需要设置 Duckov 安装路径（包含 `Duckov.app` 的目录），例如：

```bash
export DUCKOV_PATH="/path/to/Escape from Duckov"
dotnet build mods/EnderPearl/EnderPearl.csproj -c Release
```

构建完成后会自动把 `EnderPearl.dll` 复制到本目录（与 `info.ini` 同级）。

## TypeID

当前固定为：`900001`（如果与你的其他 MOD 冲突，改 [ModBehaviour.cs](ModBehaviour.cs) 里的常量即可）。
