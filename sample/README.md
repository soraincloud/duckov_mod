# sample/

本目录用于存放《逃离鸭科夫（Duckov）》官方/标准的 Mod 示例工程与文档，作为本仓库的主要参考来源。

## 你会在这里找到什么

- 官方示例根目录：`sample/duckov_modding-main/`
- 示例说明（中文）：`sample/duckov_modding-main/README.md`
- 值得注意的 API（中文）：`sample/duckov_modding-main/Documents/NotableAPIs_CN.md`

## 建议用法

- 把这里当作“对照样例/文档库”：遇到加载规则、文件结构、引用哪些 DLL 等问题，优先对照官方示例。
- 不建议直接在此目录里开发自己的 Mod（避免把示例改乱，后续也不利于同步官方更新）。

## 相关背景（简要）

官方示例中说明了 Mod 的基本加载规则（`info.ini` 的 `name` 对应 `YourMod.dll` 里的 `YourMod.ModBehaviour`）以及推荐的工程配置方式。实际开发请以示例文档为准。