# 喷溅治疗药水（SplashHealingPotion）

版本号：v1.0.2
更新日期：2026-03-18
更新内容：补充命中反馈与音效反馈，治疗药水价格减半，新增两套工作台配方、单次制作改为 2 瓶，并将模型统一调整为 66% 大小。

## Mod 简介

功能介绍：
- 新增可投掷道具「喷溅治疗药水」
- 使用手感与 EnderPearl 相同：按住显示抛物线，松手投掷
- 药水首次碰撞落地时，在一定范围内为角色恢复 50% 最大生命值
- 支持两套工作台配方，且每次制作产出 `2` 瓶
- 落地时爆开带发白高光感的莓果玫红粒子，并播放落地音效
- 当前模型统一缩放为原始大小的 `66%`

用途：
- 团队治疗
- 近距离救场
- 联机辅助或整活玩法

获取方式：
- NPC 橘子处购买（装备商人 `Merchant_Equipment`），售价 `$1000`
- 工作台配方一：`6x 止血绷带` + `2x 小急救箱` + `1x 恢复针`，产出 `2` 瓶
- 工作台配方二：`3x 玻璃` + `1x 恢复针`，产出 `2` 瓶

开发者：soraincloud  
策划：吱吱歪

声明：本 Mod 为开源项目，使用 AI 辅助开发。

补充说明：
- 第二套工作台配方依赖 `MCPrerequisite` 注册的共享物品 `玻璃`（TypeID `800001`）

## 构建

需要设置 Duckov 安装路径（包含 `Duckov.app` 的目录），例如：

```bash
export DUCKOV_PATH="/path/to/Escape from Duckov"
dotnet build mods/SplashHealingPotion/SplashHealingPotion.csproj -c Release
```

构建完成后会自动把 `SplashHealingPotion.dll` 复制到本目录（与 `info.ini` 同级）。

## 占位资源

当前资源已经切到喷溅治疗药水自己的命名：

- 图标：`assets/item-icons/SplashHealingPotion.png`
- 模型 bundle：`assets/bundles/models/splashhealingpotion_assets`
- 音效：`assets/sfx/throw.wav`

代码现在会优先加载 `SplashHealingPotion_*` 的 prefab 名，也会兼容旧的 `EnderPearl_*` 命名，便于继续迭代模型资源。
