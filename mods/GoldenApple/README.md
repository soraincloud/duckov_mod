# 金苹果（GoldenApple）

版本号：v1.0.0  
更新日期：2026-03-17

## Mod 简介

功能介绍：
- 新增可食用物品「金苹果」
- 食用后获得三段临时增益
- 兼容 MC 前置的共享分类 tag
- 不需要 3D 模型，当前使用运行时代码生成的金苹果图标

效果：
- 生命上限 `+20`，持续 2 分钟
- 30 秒内每秒回复 `5` 点生命
- 头甲 `+1.5`、身甲 `+1.5`，持续 5 分钟

说明：
- 当前版本会直接提高血量上限，但不会把新增上限单独绘制成黄色血条
- 重复食用会刷新持续时间，不会无限叠层

获取方式：
- NPC 橘子处购买（装备商人 `Merchant_Equipment`）

开发者：soraincloud  
策划：吱吱歪

声明：本 Mod 为开源项目，使用 AI 辅助开发。

## 构建

需要设置 Duckov 安装路径（包含 `Duckov.app` 的目录），例如：

```bash
export DUCKOV_PATH="/path/to/Escape from Duckov"
dotnet build mods/GoldenApple/GoldenApple.csproj -c Release
```

构建完成后会自动把 `GoldenApple.dll` 复制到本目录。

## 一键部署（本地测试）

```bash
export DUCKOV_PATH="/path/to/Escape from Duckov"
bash mods/GoldenApple/deploy.sh
```

## 图标资源

你可以先不提供外部美术资源，功能不受影响：
- 优先加载 `assets/item-icons/GoldenApple.png`
- 其次加载 `icon.png`
- 都不存在时，回退到运行时代码生成的金苹果图标

## TypeID

当前固定为：`900002`。