# EnderPearl 资源目录（assets）

这个目录用于存放 EnderPearl mod 的资源文件。

## sfx（WAV，默认启用）

把 3 个音效放在这里：

- `assets/sfx/throw.wav`
- `assets/sfx/transmit1.wav`
- `assets/sfx/transmit2.wav`

说明：

- 这些 wav 会用 **FMOD CoreSystem 直接播放**（不依赖 Unity Audio，也不需要 bank），这是目前 Duckov 里最稳定的方案。
- 如果你想更安静的日志输出：创建 `assets/sfx/verbose_sfx_log.txt`（空文件即可）来开启更详细的 SFX 日志；不创建则只保留关键日志。

## bundles

`assets/bundles/models/` 用于模型相关 AssetBundle。
