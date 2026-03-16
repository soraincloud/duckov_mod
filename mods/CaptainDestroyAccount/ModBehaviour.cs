using System.Collections.Generic;
using System.Linq;
using Duckov.UI;
using ItemStatsSystem;
using UnityEngine;

namespace CaptainDestroyAccount;

public class ModBehaviour : Duckov.Modding.ModBehaviour
{
    private const float CountdownSeconds = 10f;
    private const float RepeatedDeletionIntervalSeconds = 5f;
    private const long DeductionAmount = 168L;
    private const int CashItemTypeId = 451;

    private bool _countdownActive;
    private float _countdownDeadline;
    private bool _repeatedDeletionActive;
    private float _nextDeletionTime;
    private int _lastBroadcastSecond = -1;

    protected override void OnAfterSetup()
    {
        Debug.Log("[CaptainDestroyAccount] Loaded.");

        LevelManager.OnLevelInitialized += OnLevelInitialized;

        if (LevelManager.LevelInited)
        {
            OnLevelInitialized();
        }
    }

    protected override void OnBeforeDeactivate()
    {
        LevelManager.OnLevelInitialized -= OnLevelInitialized;
        ResetCountdown();
    }

    private void Update()
    {
        if (!LevelManager.LevelInited)
        {
            return;
        }

        if (_countdownActive)
        {
            var remainingSeconds = Mathf.Max(0, Mathf.CeilToInt(_countdownDeadline - Time.time));
            if (remainingSeconds != _lastBroadcastSecond)
            {
                _lastBroadcastSecond = remainingSeconds;
                NotificationText.Push($"由于舰长过期 毁号倒计时：{remainingSeconds} 秒");
            }

            if (Time.time >= _countdownDeadline)
            {
                _countdownActive = false;
                ApplyCountdownOutcome();
            }
        }

        if (!_repeatedDeletionActive || Time.time < _nextDeletionTime)
        {
            return;
        }

        ApplyRepeatedDeletion();
    }

    private void OnLevelInitialized()
    {
        _countdownActive = true;
        _countdownDeadline = Time.time + CountdownSeconds;
        _repeatedDeletionActive = false;
        _nextDeletionTime = 0f;
        _lastBroadcastSecond = -1;

        NotificationText.Push($"由于舰长过期 毁号倒计时已启动，{CountdownSeconds:0} 秒后扣除 ${DeductionAmount}");
    }

    private void ApplyCountdownOutcome()
    {
        var storageInventory = PlayerStorage.Inventory;
        if (storageInventory == null)
        {
            NotificationText.Push("舰长毁号触发失败：仓库未就绪");
            Debug.LogWarning("[CaptainDestroyAccount] PlayerStorage inventory missing when deduction triggered.");
            return;
        }

        if (TryConsumeStorageCash(storageInventory, DeductionAmount))
        {
            NotificationText.Push($"舰长毁号已生效：已从仓库扣除现金 ${DeductionAmount}");
            Debug.Log($"[CaptainDestroyAccount] Deducted ${DeductionAmount} cash from storage.");
            return;
        }

        _repeatedDeletionActive = true;
        _nextDeletionTime = Time.time + RepeatedDeletionIntervalSeconds;
        NotificationText.Push($"仓库现金不足，已进入持续毁号：每 {RepeatedDeletionIntervalSeconds:0} 秒随机删除一个物品");
        Debug.Log($"[CaptainDestroyAccount] Storage cash insufficient. Repeated deletion started every {RepeatedDeletionIntervalSeconds:0} seconds.");
    }

    private void ApplyRepeatedDeletion()
    {
        var storageInventory = PlayerStorage.Inventory;
        if (storageInventory == null)
        {
            _repeatedDeletionActive = false;
            _nextDeletionTime = 0f;
            NotificationText.Push("持续毁号已停止：仓库未就绪");
            Debug.LogWarning("[CaptainDestroyAccount] PlayerStorage inventory missing during repeated deletion.");
            return;
        }

        if (TryDeleteRandomStorageItem(storageInventory, out var deletedItemName))
        {
            _nextDeletionTime = Time.time + RepeatedDeletionIntervalSeconds;
            NotificationText.Push($"仓库现金不足，已随机删除：{deletedItemName}");
            Debug.Log($"[CaptainDestroyAccount] Repeated deletion removed storage item: {deletedItemName}.");
            return;
        }

        _repeatedDeletionActive = false;
        _nextDeletionTime = 0f;
        NotificationText.Push("持续毁号已停止：仓库中没有可删除的物品");
        Debug.LogWarning("[CaptainDestroyAccount] Repeated deletion stopped because storage is empty.");
    }

    private static bool TryConsumeStorageCash(Inventory storageInventory, long amount)
    {
        List<Item> cashItems = storageInventory
            .Where(item => item != null && item.TypeID == CashItemTypeId)
            .ToList();

        long totalCash = cashItems.Sum(item => (long)(item.Stackable ? item.StackCount : 1));
        if (totalCash < amount)
        {
            return false;
        }

        long remaining = amount;
        foreach (var cashItem in cashItems)
        {
            if (cashItem == null)
            {
                continue;
            }

            long itemAmount = cashItem.Stackable ? cashItem.StackCount : 1;
            if (itemAmount <= remaining)
            {
                remaining -= itemAmount;
                storageInventory.RemoveItem(cashItem);
                cashItem.DestroyTree();
            }
            else
            {
                cashItem.StackCount -= (int)remaining;
                remaining = 0L;
            }

            if (remaining <= 0)
            {
                return true;
            }
        }

        return true;
    }

    private static bool TryDeleteRandomStorageItem(Inventory storageInventory, out string deletedItemName)
    {
        List<Item> candidates = storageInventory
            .Where(item => item != null)
            .ToList();

        if (candidates.Count == 0)
        {
            deletedItemName = string.Empty;
            return false;
        }

        Item targetItem = candidates[Random.Range(0, candidates.Count)];
        deletedItemName = targetItem.DisplayName;

        if (!storageInventory.RemoveItem(targetItem))
        {
            deletedItemName = string.Empty;
            return false;
        }

        targetItem.DestroyTree();
        return true;
    }

    private void ResetCountdown()
    {
        _countdownActive = false;
        _countdownDeadline = 0f;
        _repeatedDeletionActive = false;
        _nextDeletionTime = 0f;
        _lastBroadcastSecond = -1;
    }
}