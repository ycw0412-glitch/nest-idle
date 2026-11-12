# Nest Idle 🥚

> A one-day Unity idle game prototype. Tap the egg for warmth, buy upgrades for warmth per second, and come back later — warmth keeps building while the app is closed. Every 1,000 warmth hatches a baby dinosaur.

알을 눌러 온기를 모으고, 둥지를 업그레이드하면 온기가 저절로 쌓이는 방치형 게임입니다. 앱을 꺼 둔 동안에도 온기가 쌓이고, 온기 1,000마다 알이 부화합니다. 하루 안에 만든 프로토타입입니다.

같은 공룡알 캐릭터로 만든 로블록스 오비: [egg-sand-escape](https://github.com/ycw0412-glitch/egg-sand-escape)

## 기능

| 기능 | 내용 |
|---|---|
| 탭 | 알을 누르면 온기 +1, 눌리는 연출 |
| 업그레이드 | 「둥지 짚 깔기」: 초당 온기 +1, 가격 10부터 1.15배씩 |
| 오프라인 보상 | 꺼 둔 시간 × 초당 온기, 최대 8시간 |
| 부화 | 모은 온기 1,000마다 부화, 알이 흔들림. 업그레이드에 써도 부화 진행은 줄지 않음 |
| 저장 | JSON으로 PlayerPrefs에 저장, 10초마다 자동 저장 |
| 화면 | 휴대폰 세로(1080×1920), 한글 폰트(Noto Sans KR) |

## 구조

```
Assets/Scripts/
├─ GameManager.cs    게임 상태와 흐름 (탭, 자동 증가, 부화, 저장 시점)
├─ SaveSystem.cs     저장·불러오기, 오프라인 시간 계산
├─ UpgradeSystem.cs  업그레이드 가격과 효과 (수치는 여기서만)
└─ GameUI.cs         화면 표시와 버튼 연결 (규칙은 갖지 않음)
```

규칙(GameManager·UpgradeSystem)과 화면(GameUI)을 나눴습니다. GameUI는 이벤트(`Changed`, `Hatched`, `OfflineCollected`)를 받아 그리기만 합니다.

## 오프라인 보상 설계

방치형의 핵심이라 세 가지를 지켰습니다.

1. **UTC로 잽니다.** `DateTime.UtcNow`를 써서 시간대가 바뀌어도 보상이 틀어지지 않습니다.
2. **같은 시간을 두 번 세지 않습니다.** 보상을 줄 때마다 기준 시각을 지금으로 옮깁니다. 휴대폰에서 백그라운드로 갔다 돌아올 때(`OnApplicationPause`)와 다시 켤 때 모두 받지만 중복되지 않습니다.
3. **범위를 막습니다.** 0초~8시간으로 제한해, 기기 시계를 뒤로 돌려도 음수가 되지 않고 너무 많이 쌓이지도 않습니다.

## 만들면서 푼 문제

- **한글 숫자가 안 보임:** TextMeshPro 폰트 에셋이 원본 `.ttf`와 연결이 끊겨 있어, 동적 아틀라스가 글자를 그리지 못했습니다(`3 was not found` 경고). Source Font File을 다시 연결해 해결했습니다.
- **오프라인 안내가 안 보임:** 업그레이드 버튼이 안내 글자를 덮고 있었습니다. Unity UI는 Hierarchy에서 아래에 있는 것을 위에 그리기 때문입니다. 안내 글자를 맨 아래로 옮기고 자리를 다시 잡았습니다. 시작 직후의 긴 첫 프레임에 표시 시간이 깎이지 않도록 `WaitForSecondsRealtime`으로 바꿨습니다.

## 실행

1. Unity **6000.6.9f1**(Unity 6)로 이 폴더를 엽니다.
2. `Assets/Scenes/SampleScene`을 열고 ▶ Play.
3. 오프라인 보상 확인: 업그레이드를 한 번 사고 → Play 멈춤 → 10초 뒤 다시 Play → 「쉬는 동안 온기 +10」. Console에도 `[Offline]` 기록이 남습니다.

## 다음 할 일

- [ ] 공룡 도감(부화한 공룡 5종)
- [ ] 업그레이드 여러 종류
- [ ] Blender 공룡알 렌더 이미지로 알 교체
- [ ] 저장·오프라인 계산 테스트 코드(Unity Test Framework)
- [ ] WebGL 빌드를 itch.io에 올려 브라우저에서 플레이

## 폰트

Noto Sans KR — SIL Open Font License 1.1 (`Assets/Fonts/OFL.txt`)
