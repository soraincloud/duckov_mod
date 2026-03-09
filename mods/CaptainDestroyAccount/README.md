# 舰长毁号

进入关卡后自动开始 10 秒倒计时，倒计时结束时尝试扣除 `$168`。

## 行为说明

- 触发时机：`LevelManager.OnLevelInitialized`
- 倒计时：10 秒
- 扣费：168
- 若余额与现金总额不足：不会扣款，并弹出提示

## 构建

```bash
dotnet build mods/CaptainDestroyAccount/CaptainDestroyAccount.csproj -c Release -v minimal
```

## 部署

```bash
export DUCKOV_PATH="/Volumes/Kingston-1TB/SteamLibrary/steamapps/common/Escape from Duckov"
bash mods/CaptainDestroyAccount/deploy.sh
```