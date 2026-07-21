# BGM/SFX 매니저 설계

작성일: 2026-07-21
브랜치: Easily-Typing

## 목적

게임에 배경음악(BGM)과 효과음(SFX)을 넣는다. 단, 별도의 싱글톤을
새로 만들지 않고 **GameManager와 같은 GameObject에 붙는 일반 컴포넌트**로
구현한다. 이 프로젝트는 "싱글톤은 GameManager와 PoolManager 둘로 제한"하는
원칙을 이미 세워 뒀고(`ExpGem.cs` 참고), 오디오 매니저도 그 원칙을 따른다.

## 핵심 구조

BGMManager, SFXManager는 싱글톤이 아니다. GameManager가 `Awake`에서
`GetComponent`로 둘을 잡아 프로퍼티로 노출한다. 다른 스크립트는 이미 쓰고 있는
`GameManager.Instance`를 통해 접근한다.

```csharp
GameManager.Instance.Bgm.PlayMain();
GameManager.Instance.Sfx.PlayShoot();
```

호출부는 전부 null-safe로 감싼다. 오디오 컴포넌트가 없거나 클립이 안 들어가
있어도 게임은 그대로 동작한다.

## 컴포넌트 명세

### BGMManager.cs

- `Awake`에서 `gameObject.AddComponent<AudioSource>()`로 전용 AudioSource를
  생성한다(`loop = true`, `playOnAwake = false`). 사용자가 AudioSource를
  수동으로 붙일 필요가 없다.
- 직렬화 필드: `AudioClip mainBgm`, `AudioClip bossBgm`(선택),
  `[Range(0,1)] float volume = 0.5f`
- 메서드: `PlayMain()`, `PlayBoss()`, `Stop()`, `SetVolume(float)`
- 재생하려는 클립이 null이면 조용히 넘어간다.

### SFXManager.cs

- `Awake`에서 전용 AudioSource를 생성한다(`loop = false`,
  `playOnAwake = false`). 겹쳐 나는 효과음은 `PlayOneShot`으로 처리한다.
- 직렬화 필드: `[Range(0,1)] float volume = 0.7f` + 효과음 클립 묶음:
  `shoot, hit, monsterDeath, playerHurt, gemPickup, levelUp, upgradeSelect,
  explosion, enemyShoot, gameOver, victory`
- 이름 붙은 메서드: `PlayShoot()`, `PlayHit()`, `PlayMonsterDeath()`,
  `PlayPlayerHurt()`, `PlayGemPickup()`, `PlayLevelUp()`, `PlayUpgradeSelect()`,
  `PlayExplosion()`, `PlayEnemyShoot()`, `PlayGameOver()`, `PlayVictory()`
- 각 메서드는 자기 클립을 null 체크한다. 클립을 안 넣은 효과음은 자동으로
  무음이 된다. → "모든 이벤트에 배선"하되 실제 소리 여부는 Inspector에서
  클립을 넣는 것으로 결정한다.

### GameManager.cs 수정

- `public BGMManager Bgm { get; private set; }`
- `public SFXManager Sfx { get; private set; }`
- `Awake`에서 `GetComponent`로 둘을 잡는다.
- `Start`에서 `Bgm?.PlayMain()`
- `GameOver()`에서 `Bgm?.Stop()`, `Sfx?.PlayGameOver()`
- `Victory()`에서 `Bgm?.Stop()`, `Sfx?.PlayVictory()`

## 이벤트 배선

| 이벤트 | 위치 | 호출 |
|---|---|---|
| 플레이어 발사 | `AutoAttack.Shoot` (총알 생성 후) | `PlayShoot()` |
| 총알 명중 | `Bullet.OnTriggerEnter2D` | `PlayHit()` |
| 몬스터 처치 | `MonsterController.Die` | `PlayMonsterDeath()` |
| 플레이어 피격 | `PlayerHealth.TakeDamage` (실제 피격 시) | `PlayPlayerHurt()` |
| 젬 획득 | `ExpGem.OnTriggerEnter2D` | `PlayGemPickup()` |
| 레벨업 | `PlayerLevel.LevelUp` | `PlayLevelUp()` |
| 강화 선택 | `UpgradeManager.ApplyUpgrade` | `PlayUpgradeSelect()` |
| 폭발 | `ExploderBehaviour.OnMonsterDeath` | `PlayExplosion()` |
| 적 발사 | `ShooterBehaviour.TryFire` | `PlayEnemyShoot()` |
| 게임오버 | `GameManager.GameOver` | `PlayGameOver()` |
| 승리 | `GameManager.Victory` | `PlayVictory()` |

배선 헬퍼: 매 호출부가 `GameManager.Instance`와 `Sfx` null 체크를 반복하지
않도록, 짧은 접근 패턴 `GameManager.Instance?.Sfx?.PlayXxx()`를 쓴다.
`?.`(null 조건 연산자)로 한 줄에 안전하게 처리한다.

## 씬 배선 (사용자 수동 작업)

스크립트 작성 후 Unity Editor에서:

1. Game 씬에서 GameManager가 붙은 GameObject를 선택한다.
2. `Add Component` → `BGMManager`, `SFXManager`를 추가한다.
3. 각 컴포넌트의 클립 필드에 오디오 파일(.wav/.mp3/.ogg)을 할당한다.
   넣지 않은 클립은 무음으로 남는다.
4. volume 슬라이더로 크기를 조절한다.

AudioSource는 각 컴포넌트가 런타임에 스스로 생성하므로 수동으로 붙이지 않는다.

## 범위 밖 (YAGNI)

- Intro 씬 BGM: GameManager가 Game 씬에만 있으므로 이번 범위 밖.
- 오디오 믹서/그룹, 저장되는 볼륨 설정(PlayerPrefs): 이번 범위 밖.
- 보스 등장 시 BGM 자동 전환: `PlayBoss()` API는 제공하되 자동 호출 배선은
  하지 않는다(원하면 이후 보스 스폰 지점에서 호출).
