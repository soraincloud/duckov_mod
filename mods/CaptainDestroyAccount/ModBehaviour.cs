using Duckov.Economy;
using Duckov.UI;
using UnityEngine;

namespace CaptainDestroyAccount;

public class ModBehaviour : Duckov.Modding.ModBehaviour
{
    private const float CountdownSeconds = 10f;
    private const long DeductionAmount = 168L;

    private bool _countdownActive;
    private float _countdownDeadline;
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
        if (!_countdownActive || !LevelManager.LevelInited)
        {
            return;
        }

        var remainingSeconds = Mathf.Max(0, Mathf.CeilToInt(_countdownDeadline - Time.time));
        if (remainingSeconds != _lastBroadcastSecond)
        {
            _lastBroadcastSecond = remainingSeconds;
            NotificationText.Push($"由于舰长过期 毁号倒计时：{remainingSeconds} 秒");
        }

        if (Time.time < _countdownDeadline)
        {
            return;
        }

        _countdownActive = false;
        ApplyDeduction();
    }

    private void OnLevelInitialized()
    {
        _countdownActive = true;
        _countdownDeadline = Time.time + CountdownSeconds;
        _lastBroadcastSecond = -1;

        if (EconomyManager.Instance == null)
        {
            Debug.LogWarning("[CaptainDestroyAccount] EconomyManager not ready yet. Countdown started and will retry deduction at trigger time.");
        }

        NotificationText.Push($"由于舰长过期 毁号倒计时已启动，{CountdownSeconds:0} 秒后扣除 ${DeductionAmount}");
    }

    private static void ApplyDeduction()
    {
        if (EconomyManager.Instance == null)
        {
            Debug.LogWarning("[CaptainDestroyAccount] EconomyManager missing when deduction triggered.");
            return;
        }

        var cost = new Cost(DeductionAmount);
        if (!cost.Pay())
        {
            NotificationText.Push($"舰长毁号触发失败：余额不足，无法扣除 ${DeductionAmount}");
            Debug.LogWarning($"[CaptainDestroyAccount] Not enough money to deduct ${DeductionAmount}.");
            return;
        }

        NotificationText.Push($"舰长毁号已生效：扣除 ${DeductionAmount}");
        Debug.Log($"[CaptainDestroyAccount] Deducted ${DeductionAmount}.");
    }

    private void ResetCountdown()
    {
        _countdownActive = false;
        _countdownDeadline = 0f;
        _lastBroadcastSecond = -1;
    }
}