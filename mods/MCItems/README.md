# MC物品（MCItems）

该目录是新的合并版 Mod，整合了以下四个原独立 Mod：

- 末影珍珠 `EnderPearl`
- 附魔金苹果 `GoldenApple`
- 喷溅治疗药水 `SplashHealingPotion`
- 喷溅迅捷药水 `SplashSwiftnessPotion`
- 不死图腾 `TotemOfUndying`

## 合并说明

- 游戏加载器只会根据 `info.ini` 中的 `name` 加载单一入口 `MCItems.ModBehaviour`
- 本目录通过聚合入口手动初始化四个原模块，保留它们各自的命名空间和逻辑
- 运行时资源统一放在当前目录的 `assets/` 下
- 原始独立 Mod 会移动到仓库根目录的 `archive/` 下封存

## 构建

```bash
export DUCKOV_PATH="/path/to/Escape from Duckov"
dotnet build mods/MCItems/MCItems.csproj -c Release
```

## 一键部署

```bash
export DUCKOV_PATH="/path/to/Escape from Duckov"
bash mods/MCItems/deploy.sh
```

## 发布说明

- 发布用文案见 `STEAM_DESCRIPTION.md`
- 发布预览图使用当前目录下的 `preview.png`
