# 不死图腾（TotemOfUndying）

版本号：v1.0.0  
更新日期：2026-02-28

## Mod 简介

功能介绍：
- 新增物品「不死图腾」
- 仅在图腾槽位生效（兼容 `Totem` / `SoulCube` 命名）
- 角色受到致命伤害时自动触发保命效果

触发效果：
- 消耗 1 个图腾
- 免除本次死亡
- 恢复 50% 最大生命
- 获得 3 秒无敌
- 同时爆发黄色 + 绿色粒子

获取方式：
- NPC 橘子处购买（装备商人 `Merchant_Equipment`），售价 `$300`

开发者：soraincloud  
策划：吱吱歪

声明：本 Mod 为开源项目，使用 AI 辅助开发。

## 构建

需要设置 Duckov 安装路径（包含 `Duckov.app` 的目录），例如：

```bash
export DUCKOV_PATH="/path/to/Escape from Duckov"
dotnet build mods/TotemOfUndying/TotemOfUndying.csproj -c Release
```

构建完成后会自动把 `TotemOfUndying.dll` 复制到本目录（与 `info.ini` 同级）。

## 一键部署（本地测试）

```bash
export DUCKOV_PATH="/path/to/Escape from Duckov"
bash mods/TotemOfUndying/deploy.sh
```

## 贴图/模型说明

你可以先不提供资源，功能不受影响：
- 物品贴图：不放 `icon.png` 时会使用运行时占位图标
- 3D 模型与预览图：可后续补齐

## TypeID

当前固定为：`900011`（如有冲突，可改 `ModBehaviour.cs` 中常量）。
