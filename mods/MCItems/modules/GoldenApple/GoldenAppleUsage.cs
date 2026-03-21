using ItemStatsSystem;

namespace GoldenApple;

/// <summary>
/// 金苹果的使用行为，只负责把效果交给 BuffRegistry，具体数值逻辑不放在物品层。
/// </summary>
public class GoldenAppleUsage : UsageBehavior
{
    public override DisplaySettingsData DisplaySettings => new()
    {
        display = true,
        description = "+20 生命上限 120秒 / 每秒回血15 持续30秒 / 头甲+1.5 身甲+1.5 持续300秒\n左下角状态栏会显示增益与剩余时间"
    };

    public override bool CanBeUsed(Item item, object user)
    {
        return user is CharacterMainControl;
    }

    protected override void OnUse(Item item, object user)
    {
        if (user is not CharacterMainControl character)
        {
            return;
        }

        GoldenAppleBuffRegistry.ApplyTo(character);
    }
}