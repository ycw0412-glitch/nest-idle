using System;
using UnityEngine;

/// <summary>
/// 저장되는 게임 상태. JsonUtility로 저장하려고 public 필드로 둔다.
/// </summary>
[Serializable]
public class GameData
{
    public double warmth;        // 쓸 수 있는 온기
    public double hatchProgress; // 부화까지 모은 온기 (업그레이드에 써도 줄지 않는다)
    public int strawLevel;       // 「둥지 짚 깔기」 업그레이드 단계
    public int hatchCount;       // 부화시킨 알 수
}

/// <summary>
/// 게임 상태와 흐름: 탭, 자동 증가, 부화, 오프라인 보상, 저장 시점.
/// 화면은 GameUI가 이벤트를 받아 그린다.
/// </summary>
public class GameManager : MonoBehaviour
{
    public const double TapWarmth = 1;
    public const double HatchGoal = 1000;
    const float AutosaveSeconds = 10f;

    public GameData Data { get; private set; }
    public double WarmthPerSecond => UpgradeSystem.WarmthPerSecond(Data.strawLevel);
    public float HatchRatio => (float)(Data.hatchProgress / HatchGoal);

    public event Action Changed;                 // 숫자가 바뀔 때마다
    public event Action<int> Hatched;            // 부화했을 때 (몇 번째인지)
    public event Action<double> OfflineCollected; // 꺼 둔 동안 쌓인 온기를 받았을 때

    float autosaveTimer;
    bool started;

    void Awake()
    {
        Data = SaveSystem.Load();
    }

    void Start()
    {
        CollectOffline();
        started = true;
        Changed?.Invoke();
    }

    void Update()
    {
        double perSecond = WarmthPerSecond;
        if (perSecond > 0)
        {
            AddWarmth(perSecond * Time.deltaTime);
            Changed?.Invoke();
        }

        autosaveTimer += Time.deltaTime;
        if (autosaveTimer >= AutosaveSeconds)
        {
            autosaveTimer = 0f;
            Save();
        }
    }

    public void Tap()
    {
        AddWarmth(TapWarmth);
        Changed?.Invoke();
    }

    public bool BuyStraw()
    {
        if (!UpgradeSystem.TryBuyStraw(Data)) return false;
        Save();
        Changed?.Invoke();
        return true;
    }

    // 테스트용: 저장을 지우고 처음부터
    public void ResetGame()
    {
        SaveSystem.ResetAll();
        Data = new GameData();
        Changed?.Invoke();
    }

    void AddWarmth(double amount)
    {
        Data.warmth += amount;
        Data.hatchProgress += amount;
        while (Data.hatchProgress >= HatchGoal)
        {
            Data.hatchProgress -= HatchGoal;
            Data.hatchCount++;
            Hatched?.Invoke(Data.hatchCount);
        }
    }

    void CollectOffline()
    {
        double seconds = SaveSystem.OfflineSeconds();
        double earned = SaveSystem.CollectOffline(WarmthPerSecond);
        Debug.Log($"[Offline] 쉬던 시간 {seconds:0.0}초 × 초당 {WarmthPerSecond} = 온기 +{earned:0}");
        if (earned < 1) return;
        AddWarmth(earned);
        OfflineCollected?.Invoke(earned);
        Changed?.Invoke();
    }

    void Save() => SaveSystem.Save(Data);

    // 휴대폰에서는 앱을 끄는 대신 백그라운드로 보내는 경우가 많아 둘 다 저장한다.
    void OnApplicationPause(bool paused)
    {
        if (paused)
        {
            Save();
        }
        else if (started)
        {
            CollectOffline(); // 백그라운드에 있던 동안 쌓인 온기
        }
    }

    void OnApplicationQuit() => Save();
}
