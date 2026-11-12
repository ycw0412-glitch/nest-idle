using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 화면 그리기와 버튼 연결. 게임 규칙은 GameManager·UpgradeSystem에 있고 여기서는 보여 주기만 한다.
/// </summary>
public class GameUI : MonoBehaviour
{
    [SerializeField] GameManager game;

    [Header("알")]
    [SerializeField] Button eggButton;
    [SerializeField] RectTransform eggVisual; // 눌림·흔들림 연출을 줄 알 이미지

    [Header("글자")]
    [SerializeField] TMP_Text warmthText;
    [SerializeField] TMP_Text perSecondText;
    [SerializeField] TMP_Text hatchCountText;
    [SerializeField] TMP_Text messageText;

    [Header("부화 게이지 (Image Type = Filled)")]
    [SerializeField] Image hatchFill;

    [Header("업그레이드")]
    [SerializeField] Button strawButton;
    [SerializeField] TMP_Text strawButtonText;

    [Header("테스트용 (비워 둬도 됨)")]
    [SerializeField] Button resetButton;

    static readonly string[] Suffixes = { "", "K", "M", "B", "T", "aa", "ab", "ac" };

    Vector3 eggBaseScale;
    Coroutine messageRoutine;
    Coroutine popRoutine;

    void Awake()
    {
        // GameManager.Start보다 먼저 구독해야 오프라인 보상 알림을 놓치지 않는다.
        game.Changed += Refresh;
        game.Hatched += OnHatched;
        game.OfflineCollected += OnOfflineCollected;

        eggButton.onClick.AddListener(OnEggTapped);
        strawButton.onClick.AddListener(OnStrawClicked);
        if (resetButton != null) resetButton.onClick.AddListener(game.ResetGame);

        eggBaseScale = eggVisual.localScale;
        if (messageText != null) messageText.text = "";
    }

    void OnDestroy()
    {
        if (game == null) return;
        game.Changed -= Refresh;
        game.Hatched -= OnHatched;
        game.OfflineCollected -= OnOfflineCollected;
    }

    void Start() => Refresh();

    void Refresh()
    {
        GameData data = game.Data;
        if (data == null) return;

        warmthText.text = $"온기 {Format(data.warmth)}";
        perSecondText.text = $"초당 +{Format(game.WarmthPerSecond)}";
        hatchCountText.text = $"부화 {data.hatchCount}";
        hatchFill.fillAmount = game.HatchRatio;

        double cost = UpgradeSystem.StrawCost(data.strawLevel);
        strawButtonText.text = $"둥지 짚 깔기 Lv{data.strawLevel}\n{Format(cost)} 온기 · 초당 +{UpgradeSystem.WarmthPerStraw}";
        strawButton.interactable = UpgradeSystem.CanBuyStraw(data);
    }

    void OnEggTapped()
    {
        game.Tap();
        if (popRoutine != null) StopCoroutine(popRoutine);
        popRoutine = StartCoroutine(Pop());
    }

    void OnStrawClicked()
    {
        if (!game.BuyStraw()) ShowMessage("온기가 모자라요");
    }

    void OnHatched(int count)
    {
        ShowMessage($"부화했어요! 아기 공룡 {count}마리째");
        StartCoroutine(Shake());
    }

    void OnOfflineCollected(double earned)
    {
        // 앱을 켜자마자 지나가지 않게 오래 보여 준다
        ShowMessage($"쉬는 동안 온기 +{Format(earned)}", 5f);
    }

    void ShowMessage(string text, float seconds = 2.5f)
    {
        if (messageText == null) return;
        if (messageRoutine != null) StopCoroutine(messageRoutine);
        messageRoutine = StartCoroutine(MessageFor(text, seconds));
    }

    IEnumerator MessageFor(string text, float seconds)
    {
        messageText.text = text;
        // 시작 직후의 긴 첫 프레임에 시간이 먹히지 않도록 실제 경과 시간으로 잰다
        yield return new WaitForSecondsRealtime(seconds);
        messageText.text = "";
    }

    // 누르면 살짝 작아졌다가 돌아온다
    IEnumerator Pop()
    {
        const float duration = 0.12f;
        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            float k = 1f - 0.1f * Mathf.Sin(t / duration * Mathf.PI);
            eggVisual.localScale = eggBaseScale * k;
            yield return null;
        }
        eggVisual.localScale = eggBaseScale;
    }

    // 부화할 때 좌우로 흔들린다
    IEnumerator Shake()
    {
        const float duration = 0.6f;
        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            float angle = Mathf.Sin(t * 50f) * 12f * (1f - t / duration);
            eggVisual.localRotation = Quaternion.Euler(0, 0, angle);
            yield return null;
        }
        eggVisual.localRotation = Quaternion.identity;
    }

    /// <summary>1234 → 1.23K, 5600000 → 5.6M 처럼 줄여 쓴다.</summary>
    public static string Format(double value)
    {
        if (value < 1000) return Math.Floor(value).ToString("0");
        int index = 0;
        while (value >= 1000 && index < Suffixes.Length - 1)
        {
            value /= 1000;
            index++;
        }
        string pattern = value < 10 ? "0.##" : value < 100 ? "0.#" : "0";
        return value.ToString(pattern) + Suffixes[index];
    }
}
