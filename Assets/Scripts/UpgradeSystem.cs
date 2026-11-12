using System;

/// <summary>
/// 업그레이드 가격과 효과. 수치는 여기서만 바꾼다.
/// 하루 버전은 「둥지 짚 깔기」 하나: 살 때마다 초당 온기 +1, 가격 1.15배.
/// </summary>
public static class UpgradeSystem
{
    public const double StrawBaseCost = 10;
    public const double StrawCostGrowth = 1.15;
    public const double WarmthPerStraw = 1;

    public static double StrawCost(int level) => Math.Floor(StrawBaseCost * Math.Pow(StrawCostGrowth, level));

    public static double WarmthPerSecond(int strawLevel) => strawLevel * WarmthPerStraw;

    public static bool CanBuyStraw(GameData data) => data.warmth >= StrawCost(data.strawLevel);

    public static bool TryBuyStraw(GameData data)
    {
        double cost = StrawCost(data.strawLevel);
        if (data.warmth < cost) return false;
        data.warmth -= cost;
        data.strawLevel++;
        return true;
    }
}
