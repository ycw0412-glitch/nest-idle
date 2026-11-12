using System;
using UnityEngine;

/// <summary>
/// 저장·불러오기와 오프라인 시간 계산. PlayerPrefs에 JSON으로 저장한다.
/// </summary>
public static class SaveSystem
{
    const string DataKey = "nestIdle.data";
    const string LastSeenKey = "nestIdle.lastSeenUtc";
    public const double MaxOfflineSeconds = 8 * 60 * 60; // 최대 8시간

    public static GameData Load()
    {
        string json = PlayerPrefs.GetString(DataKey, "");
        if (string.IsNullOrEmpty(json)) return new GameData();
        try
        {
            return JsonUtility.FromJson<GameData>(json) ?? new GameData();
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[Save] 저장 데이터를 읽지 못해 새로 시작합니다: {e.Message}");
            return new GameData();
        }
    }

    public static void Save(GameData data)
    {
        PlayerPrefs.SetString(DataKey, JsonUtility.ToJson(data));
        MarkSeen();
        PlayerPrefs.Save();
    }

    /// <summary>
    /// 마지막으로 본 시각부터 지금까지 쌓인 온기를 돌려주고, 기준 시각을 지금으로 옮긴다.
    /// 여러 번 불려도 같은 시간을 두 번 세지 않는다.
    /// </summary>
    public static double CollectOffline(double warmthPerSecond)
    {
        double seconds = OfflineSeconds();
        MarkSeen();
        PlayerPrefs.Save();
        return seconds * warmthPerSecond;
    }

    public static double OfflineSeconds()
    {
        string raw = PlayerPrefs.GetString(LastSeenKey, "");
        if (!long.TryParse(raw, out long binary)) return 0;
        DateTime lastSeen = DateTime.FromBinary(binary);
        double seconds = (DateTime.UtcNow - lastSeen).TotalSeconds;
        return Math.Clamp(seconds, 0, MaxOfflineSeconds); // 시계를 뒤로 돌려도 음수가 되지 않게
    }

    public static void ResetAll()
    {
        PlayerPrefs.DeleteKey(DataKey);
        PlayerPrefs.DeleteKey(LastSeenKey);
        PlayerPrefs.Save();
    }

    // 시간대가 바뀌어도 틀어지지 않게 UTC로 잰다.
    static void MarkSeen() => PlayerPrefs.SetString(LastSeenKey, DateTime.UtcNow.ToBinary().ToString());
}
