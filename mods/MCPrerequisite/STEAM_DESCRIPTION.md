# MC前置 | MCPrerequisite

## 中文介绍

版本：v1.0.0
更新日期：2026-03-18
更新内容：补充共享材料物品说明，当前直接托管玻璃、铁锭、金锭 3 个 MC 材料物品，并同步支持金苹果等依赖前置配方的说明。

这是 MC 系列 Mod 的公共前置，用来统一管理工作台与仓库中的 `MC` 分类显示逻辑，并直接注册共享材料物品 `玻璃`、`铁锭`、`金锭`。启用本前置后，已接入的 MC 系列物品会在工作台和仓库中共用同一套 `MC` 分类入口；未启用前置时，这个分类不会出现，避免每个独立 Mod 各自重复维护同一套分类按钮、图标与标签逻辑。

当前已接入：

- EnderPearl
- GoldenApple
- Splash Healing Potion
- Totem Of Undying

### 功能

- 为已接入的 MC 系列 Mod 统一提供工作台 `MC` 分类
- 为已接入的 MC 系列 Mod 统一提供仓库 `MC` 分类
- 直接托管 3 个共享 MC 材料物品：`玻璃`、`铁锭`、`金锭`
- 为共享材料物品统一提供仓库 `MC材料` 分类
- 统一维护共享分类图标与共享标签逻辑
- 在切换场景、仓库加载后自动重新应用分类状态
- 减少各个 MC 系列 Mod 重复实现分类 UI 的成本

### 说明

- 需要与已接入的 MC 系列 Mod 搭配使用
- `GoldenApple`、`Splash Healing Potion` 等工作台配方可以直接复用本前置托管的共享材料物品
- 关闭本前置后，工作台与仓库中的 `MC` 分类都会一并消失

### 开发信息

开发者：soraincloud  
策划：吱吱歪  
声明：本 Mod 为开源项目，使用 AI 辅助开发。

## English Description

Version: v1.0.0
Updated: 2026-03-18
Update Notes: Updated the description to reflect the three shared MC material items managed directly by this prerequisite and the recipe dependencies used by mods like GoldenApple.

This is the shared prerequisite mod for the MC series. It centralizes the `MC` category logic used by both the workbench and storage UI, and directly registers three shared material items: `Glass`, `Iron Ingot`, and `Gold Ingot`. When this prerequisite is enabled, supported MC-series items share one unified `MC` category entry. When it is disabled, that category will not appear, which avoids making every standalone mod maintain its own duplicate category buttons, icons, and tag logic.

Currently integrated:

- EnderPearl
- GoldenApple
- Splash Healing Potion
- Totem Of Undying

### Features

- Provides a unified `MC` category in the workbench for supported MC-series mods
- Provides a unified `MC` category in storage UI for supported MC-series mods
- Directly manages three shared MC material items: `Glass`, `Iron Ingot`, and `Gold Ingot`
- Provides a dedicated `MC材料` storage category for those shared materials
- Centralizes the shared category icon and shared-tag logic
- Reapplies category state automatically after scene changes and storage reloads
- Reduces duplicated category UI logic across individual MC-series mods

### Notes

- It is intended to be used together with supported MC-series mods
- Workbench recipes from mods like `GoldenApple` and `Splash Healing Potion` can reuse the shared material items managed here
- If this prerequisite is disabled, the `MC` category disappears from both the workbench and storage UI

### Credits

Developer: soraincloud  
Design: 吱吱歪  
Disclaimer: This mod is open source and was developed with AI assistance.