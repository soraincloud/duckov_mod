# 舰长毁号

进入关卡后自动开始 10 秒倒计时，倒计时结束时优先扣除仓库中的 `$168` 现金物品。

## 行为说明

- 触发时机：`LevelManager.OnLevelInitialized`
- 倒计时：10 秒
- 扣费来源：仓库中的现金物品
- 扣费金额：168
- 若仓库现金不足：随机删除仓库中的一个物品，并弹出提示显示物品名称

## 构建

```bash
dotnet build mods/CaptainDestroyAccount/CaptainDestroyAccount.csproj -c Release -v minimal
```

## 部署

```bash
export DUCKOV_PATH="/Volumes/Kingston-1TB/SteamLibrary/steamapps/common/Escape from Duckov"
bash mods/CaptainDestroyAccount/deploy.sh
```