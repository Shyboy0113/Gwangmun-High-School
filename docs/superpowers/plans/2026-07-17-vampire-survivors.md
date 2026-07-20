# 뱀서라이크 전환 구현 계획

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 횡스크롤 플랫포머 프로토타입을 탑다운 뱀서라이크 강사용 정답본으로 전환한다.

**Architecture:** 설계 문서 `docs/superpowers/specs/2026-07-17-vampire-survivors-design.md`를 따른다.
싱글톤 2개(`GameManager`, `PoolManager`), 오브젝트 풀링, 심플 팩토리를 배포본 인프라에만 적용한다.
몬스터는 프리팹 3종(추격·사격·폭발) + `MonsterData` 주입 구조다.

**Tech Stack:** Unity 6000.3.12f1, C#, URP 17.3.0, TextMeshPro, Unity Test Framework 1.6.0

## 구현 완료 요약 (2026-07-18)

전 태스크 구현 완료. 검증은 **배치 모드 컴파일 + 에디터 스크립트 실행**으로 했다
(Task 0대로 테스트 러너는 한글 경로 문제로 불가).

- Task 1~14: 스크립트 18종 구현, 최종 컴파일 `error CS` 0건
- Task 16: 에디터 스크립트로 프리팹 5종·데이터 9종 생성, 씬 배선(참조 누락 0)
- Task 15: Ball 3파일 삭제

**설계에서 벗어난 점 2가지:**
1. **테스트 없이 진행** — 한글 경로로 테스트 러너 불가 (Task 0). 컴파일 검증만.
2. **Ball.prefab도 삭제** — 설계는 프리팹 보존을 제안했으나, Ball.cs 삭제 시 깨진 스크립트
   프리팹이 되어 콘솔 경고를 유발. 셋을 함께 삭제(git 이력 보존).

**미완 (사람이 할 일):** ① 에디터에서 Play 검증 (나는 불가), ② 학생 배포본 추출,
③ 수치 밸런싱, ④ UI/스프라이트 미화. 상세는 `docs/HANDOFF.md`.

---

## Global Constraints

- Unity 에디터 버전: **6000.3.12f1** (설치 경로 `C:\Program Files\Unity\Hub\Editor\6000.3.12f1\Editor\Unity.exe`)
- **기존 `.cs` 파일은 경로를 바꾸지 않는다.** 프리팹·씬이 `.meta`의 GUID로 스크립트를 참조하므로
  제자리에서 내용만 교체한다. 파일을 옮겨야 하면 `.cs`와 `.cs.meta`를 **반드시 함께** `git mv` 한다.
- 알려진 GUID (건드리면 씬/프리팹 참조가 끊어짐):
  | GUID | 파일 | 참조하는 곳 |
  |---|---|---|
  | `49fb29635fc643541828554edb873051` | `PlayerController.cs` | TestScene:624 |
  | `518fc39c49e6d5b43a0dd67489d2ee74` | `PlayerShooter.cs` | TestScene:638 |
  | `a334a421334394c43b7330687a358889` | `BallSpawner.cs` | TestScene:1772 |
  | `5563ce313fb2b40449c3053e0ead8fd0` | `MonsterSpawner.cs` | TestScene:1901 |
  | `0aacb09d1d5300d44a20d594fa152394` | `GameManager.cs` | TestScene:2621 |
  | `c289d525088c7a44fb79f8bc558b9458` | `MonsterController.cs` | Monster.prefab:179 |
  | `9392e8c12953db344ad7e7f4150d4a0c` | `Bullet.cs` | Bullet.prefab |
- **네임스페이스를 쓰지 않는다.** 기존 코드가 전역 클래스이고, 학생이 읽을 코드다.
- 클래스명은 파일명과 반드시 일치한다 (Unity MonoBehaviour 요구사항).
- 학생 빈칸 7개는 아래 주석 표식으로 감싼다. 이 표식으로 학생 배포본을 기계적으로 뽑아낼 수 있다:
  ```csharp
  // ─────────── 학생 빈칸 N (M일차) ───────────
  //   ... 구현 ...
  // ──────────────────────────────────────────
  ```
- 태그: `Player`, `Monster`, `Bullet` 사용. 새 태그가 필요하면 `ProjectSettings/TagManager.asset`에 추가한다.

## 검증 명령 (모든 태스크 공통)

**컴파일 검증** — 스크립트를 쓴 뒤 반드시 실행:

```bash
cd "C:/Users/ghdrl/Downloads/2026 SW중심대학 활동/2026.08 Gwangmun HighSchool/Gwangmun-High-School"
SCRATCH="C:/Users/ghdrl/AppData/Local/Temp/claude/C--Users-ghdrl-Downloads-2026-SW--------2026-08-Gwangmun-HighSchool-Gwangmun-High-School/0b72f609-cddd-47f1-b2fc-335f48531db1/scratchpad"
"/c/Program Files/Unity/Hub/Editor/6000.3.12f1/Editor/Unity.exe" -batchmode -quit -nographics \
  -projectPath "$(pwd -W)" -logFile "$SCRATCH/compile.log"
echo "EXIT: $?"
grep -E "error CS" "$SCRATCH/compile.log" || echo "컴파일 에러 없음"
```

기대 결과: `EXIT: 0`, `컴파일 에러 없음`.

**베이스라인(작업 전 상태)은 검증됨** — `Assembly-CSharp.dll`이 정상 빌드됨을 확인했다.
따라서 이후 발생하는 `error CS`는 전부 이 계획의 변경으로 인한 것이다.

**테스트 실행** (Task 0에서 사용 가능 여부 판정):

```bash
"/c/Program Files/Unity/Hub/Editor/6000.3.12f1/Editor/Unity.exe" -batchmode -runTests \
  -testPlatform EditMode -projectPath "$(pwd -W)" \
  -testResults "$SCRATCH/results.xml" -logFile "$SCRATCH/test.log" -nographics
```

---

## Task 0: 테스트 인프라 판정 — ❌ 실패, 테스트 없이 진행

**실행 결과 (2026-07-17):** 스모크 테스트를 asmdef 2개와 함께 구성해 실행한 결과,
Unity가 아래 예외를 무한 반복하며 멈춰 테스트 러너가 완료되지 않았다. 결과 XML도 생성되지 않았다.

```
ExecutionEngineException: String conversion error: Illegal byte sequence encounted in the input.
  at System.Reflection.RuntimeAssembly.get_code_base(...)
  at UnityEditor.QuickInstall.QuickInstaller.DetectPackagesFromLoadedAssemblies()
  at UnityEditor.EditorApplication.Internal_CallUpdateFunctions()
```

**원인:** 프로젝트 경로에 한글이 포함돼 있다.

```
C:\Users\ghdrl\Downloads\2026 SW중심대학 활동\2026.08 Gwangmun HighSchool\Gwangmun-High-School
                          ^^^^^^^^^^^^^^^^
```

Mono가 어셈블리의 `code_base`를 읽을 때 한글 경로의 바이트 시퀀스에서 터진다.
`QuickInstaller`가 `EditorApplication` 업데이트 콜백에서 이 코드를 반복 호출하므로
에디터가 안정 상태에 도달하지 못하고, 테스트 러너는 영원히 끝나지 않는다.

**왜 베이스라인 임포트는 성공했나:** `-batchmode -quit`은 임포트 직후 종료하므로
패키지 매니저 콜백이 루프에 빠지기 전에 프로세스가 끝난다. `-runTests`는 에디터를
계속 살려둬야 해서 루프에 걸린다.

**결정:** asmdef 2개와 `Assets/Tests/`를 삭제하고 **컴파일 검증만으로 진행한다.**
얻는 것 없이 학생이 열어볼 폴더에 이해 못 할 파일만 남기고, 기존 코드의 어셈블리 소속을
바꿔 위험만 더하기 때문이다. (제거 완료)

**결과적으로 이 계획의 검증 수단은 배치 모드 컴파일 하나뿐이다.**
컴파일이 통과해도 게임이 실제로 도는지는 별개 문제이며, Task 17에서 이를 명시적으로 보고한다.

**후속 제안 (사람이 할 일):** 프로젝트를 한글 없는 경로(예: `C:\Dev\GwangmunHigh`)로 옮기면
테스트 러너가 살아날 가능성이 높다. 수업 배포본을 만들 때도 한글 경로를 피하는 편이 안전하다.

- [x] Step 1: 스모크 테스트 실행 → 무한 루프로 실패
- [x] Step 2: 판정 → 테스트 인프라 제거 완료
- [x] Step 3: 커밋

---

## Task 1: PlayerStats + PlayerController (학생 빈칸 1)

**Files:**
- Create: `Assets/Scripts/PlayerStats.cs`
- Modify: `Assets/Scripts/PlayerController.cs` (전체 교체, 경로·GUID 유지)

**Interfaces:**
- Produces:
  - `PlayerStats` — public 필드 `moveSpeed`(float), `damage`(float), `fireRate`(float),
    `bulletCount`(int), `maxHp`(int), `magnetRadius`(float)
  - `PlayerController.FacingDirection` → `Vector2` (읽기 전용 프로퍼티)

- [ ] **Step 1: `PlayerStats.cs` 작성**

```csharp
using UnityEngine;

/// 플레이어의 모든 수치를 한곳에 모아둔 창고.
/// 강화 카드는 이 숫자들만 올린다. 다른 스크립트는 여기서 읽기만 한다.
public class PlayerStats : MonoBehaviour
{
    [Header("이동")]
    public float moveSpeed = 5f;

    [Header("공격")]
    public float damage      = 1f;
    public float fireRate    = 0.3f;   // 발사 간격(초). 작을수록 빠름
    public int   bulletCount = 1;

    [Header("생존")]
    public int maxHp = 10;

    [Header("수집")]
    public float magnetRadius = 2f;
}
```

- [ ] **Step 2: `PlayerController.cs` 전체 교체**

중력·점프·`isGrounded`·Ground 태그를 전부 제거한다. `Move()`가 학생 빈칸 1이다.

- [ ] **Step 3: 컴파일 검증** — 위 공통 명령. 기대: `error CS` 없음.

- [ ] **Step 4: 커밋**

---

## Task 2: PoolManager (싱글톤 + 풀링)

**Files:**
- Create: `Assets/Scripts/PoolManager.cs`
- Test: `Assets/Tests/EditMode/PoolManagerTests.cs` (Task 0 통과 시에만)

**Interfaces:**
- Produces:
  - `PoolManager.Instance` → `PoolManager`
  - `Get(GameObject prefab, Vector3 position)` → `GameObject`
  - `Release(GameObject obj)` → `void`
  - `CountInactive(GameObject prefab)` → `int` (테스트·검증용)
  - `[SerializeField] bool poolingEnabled` — 3일차 시연용 토글

- [ ] **Step 1: 실패하는 테스트 작성** (Task 0 통과 시)

```csharp
using NUnit.Framework;
using UnityEngine;

public class PoolManagerTests
{
    private PoolManager pool;
    private GameObject prefab;

    [SetUp]
    public void SetUp()
    {
        pool   = new GameObject("Pool").AddComponent<PoolManager>();
        prefab = new GameObject("Prefab");
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(pool.gameObject);
        Object.DestroyImmediate(prefab);
    }

    [Test]
    public void Release_ThenGet_ReusesSameInstance()
    {
        GameObject first = pool.Get(prefab, Vector3.zero);
        pool.Release(first);
        GameObject second = pool.Get(prefab, Vector3.zero);
        Assert.AreSame(first, second, "풀이 인스턴스를 재사용해야 한다");
    }

    [Test]
    public void Get_WhenPoolEmpty_CreatesNewInstance()
    {
        GameObject a = pool.Get(prefab, Vector3.zero);
        GameObject b = pool.Get(prefab, Vector3.zero);
        Assert.AreNotSame(a, b, "풀이 비었으면 새로 만들어야 한다");
    }

    [Test]
    public void Release_Twice_DoesNotDoubleEnqueue()
    {
        GameObject obj = pool.Get(prefab, Vector3.zero);
        pool.Release(obj);
        pool.Release(obj);   // 이중 반납
        Assert.AreEqual(1, pool.CountInactive(prefab), "같은 객체가 두 번 들어가면 안 된다");
    }

    [Test]
    public void Get_SetsPosition()
    {
        GameObject obj = pool.Get(prefab, new Vector3(5f, 3f, 0f));
        Assert.AreEqual(new Vector3(5f, 3f, 0f), obj.transform.position);
    }
}
```

이중 반납 테스트가 핵심이다. 설계 문서 검증 항목 4가 지적한 실제 버그다.

- [ ] **Step 2: 테스트 실패 확인** — 기대: `PoolManager` 타입 없음으로 컴파일 실패.

- [ ] **Step 3: `PoolManager.cs` 구현**

이중 반납은 `activeSelf`로 막는다. 비활성 객체를 또 반납하면 무시한다.

- [ ] **Step 4: 테스트 통과 확인** — 기대: 4개 통과.

- [ ] **Step 5: 커밋**

---

## Task 3: MonsterData + MonsterBehaviour + ChaseBehaviour (학생 빈칸 2)

**Files:**
- Create: `Assets/Scripts/MonsterData.cs`
- Create: `Assets/Scripts/MonsterBehaviour.cs`
- Create: `Assets/Scripts/ChaseBehaviour.cs`

**Interfaces:**
- Consumes: `GameManager.Instance.IsPlaying`
- Produces:
  - `enum MonsterBehaviorType { Chase, Shooter, Exploder }`
  - `MonsterData` (ScriptableObject) — 필드는 설계 문서 6절과 **정확히 일치**
  - `abstract MonsterBehaviour.Init(MonsterData)` → `void`
  - `abstract MonsterBehaviour.Tick()` → `void` (protected)
  - `virtual MonsterBehaviour.OnMonsterDeath()` → `void`
  - `protected static MonsterBehaviour.FindPlayer()` → `Transform`

- [ ] **Step 1: `MonsterData.cs` 작성** — 설계 문서 6절 코드 그대로.

- [ ] **Step 2: `MonsterBehaviour.cs` 작성**

플레이어 참조는 **static 캐시**로 둔다. 풀링 때문에 `Init()`이 스폰마다 호출되는데
매번 `FindGameObjectWithTag`를 부르면 초당 수십 번이 된다.
Unity의 fake-null 덕분에 씬이 다시 로드되면 캐시가 자동으로 무효화된다.

- [ ] **Step 3: `ChaseBehaviour.cs` 작성** — `Chase()`가 학생 빈칸 2.

- [ ] **Step 4: 컴파일 검증**

- [ ] **Step 5: 커밋**

---

## Task 4: MonsterController (공통 부분)

**Files:**
- Modify: `Assets/Scripts/MonsterController.cs` (전체 교체, GUID 유지)

**Interfaces:**
- Consumes: `MonsterData`, `MonsterBehaviour.Init`, `PoolManager.Instance.Release`,
  `ExpGem.Init(int)`, `PlayerHealth.TakeDamage(int)`
- Produces:
  - `MonsterController.Init(MonsterData)` → `void`
  - `MonsterController.TakeDamage(float)` → `void`

**주의:** `ExpGem`과 `PlayerHealth`는 Task 8·9에서 만든다. 이 태스크만으로는 컴파일되지 않는다.
**Task 4·8·9를 한 묶음으로 구현하고 그 끝에서 컴파일 검증한다.**

- [ ] **Step 1: 콜라이더 방식 확인**

```bash
grep -A3 "Collider2D" Assets/Monster.prefab | grep -E "m_IsTrigger|m_Radius"
```

기존 `MonsterController`가 `OnTriggerEnter2D`로 총알을 받았으므로 트리거일 것으로 예상된다.
트리거면 플레이어 접촉도 `OnTriggerEnter2D`/`OnTriggerStay2D`로 처리한다.
**확인 결과에 맞춰 구현한다. 추측하지 않는다.**

- [ ] **Step 2: `MonsterController.cs` 전체 교체**

책임: 체력, 피격, 사망, 젬 드롭, 접촉 데미지. **이동·공격은 하지 않는다.**
`Init()` 끝에서 `GetComponent<MonsterBehaviour>().Init(data)`로 데이터를 넘긴다.
`Stay`로도 접촉 데미지를 준다 — 뱀서라이크는 붙어 있으면 계속 아프다.
`PlayerHealth`의 무적 시간이 연사를 막는다.

- [ ] **Step 3: 커밋** (컴파일 검증은 Task 9 이후)

---

## Task 5: MonsterFactory (심플 팩토리)

**Files:**
- Create: `Assets/Scripts/MonsterFactory.cs`

**Interfaces:**
- Consumes: `PoolManager.Instance.Get`, `MonsterController.Init`, `MonsterBehaviorType`
- Produces: `MonsterFactory.Create(MonsterData data, Vector3 pos)` → `GameObject`

- [ ] **Step 1: `MonsterFactory.cs` 작성** — 설계 문서 6절 코드 그대로.

`SelectPrefab`의 `default`는 `chasePrefab`이다. enum이 늘어나도 null을 반환하지 않는다.

- [ ] **Step 2: 커밋**

---

## Task 6: MonsterSpawner (원형 스폰 + appearTime)

**Files:**
- Modify: `Assets/Scripts/MonsterSpawner.cs` (전체 교체, GUID 유지)

**Interfaces:**
- Consumes: `MonsterFactory.Create`, `GameManager.Instance.SurvivedTime`, `MonsterData.appearTime`
- Produces: 없음 (씬에만 존재)

- [ ] **Step 1: 전체 교체**

책임은 **언제·어디에**만이다. 무엇을 만들지는 팩토리에 위임한다.
- 스폰 위치: 플레이어 중심 반경 `spawnRadius`의 원 둘레 랜덤
- `appearTime`이 지난 몬스터만 후보에 넣는다
- 후보가 없으면 스폰하지 않는다 (초반에 `appearTime: 0` 몬스터가 최소 1종 필요)
- `maxAliveMonsters` 상한을 둔다 (학교 노트북 보호)
- 기존 난이도 곡선(`intervalDecreaseRate`, `minInterval`)은 유지하되
  `spawnInterval` 원본을 보존하고 `currentInterval`을 따로 둔다.
  **기존 코드는 직렬화 필드를 직접 깎아서 에디터에서 플레이할 때마다 값이 줄어드는 버그가 있었다.**

- [ ] **Step 2: 커밋**

---

## Task 7: Bullet + AutoAttack (학생 빈칸 3)

**Files:**
- Modify: `Assets/Scripts/Bullet.cs` (전체 교체, GUID 유지)
- Modify: `Assets/Scripts/PlayerShooter.cs` → `Assets/Scripts/AutoAttack.cs`
  (`git mv` 로 `.cs`와 `.cs.meta` **함께** 이동. GUID `518fc39c...` 유지 → 씬 참조 보존)

**Interfaces:**
- Consumes: `PlayerStats`, `PoolManager`, `MonsterController.TakeDamage(float)`
- Produces:
  - `Bullet.Initialize(Vector2 direction, float damage)` → `void`
  - `AutoAttack.FindNearestMonster()` → `Transform` (학생 빈칸 3)

- [ ] **Step 1: `Bullet.cs` 전체 교체**

`Destroy(gameObject, lifeTime)` → 풀 반납으로 교체한다. `Update()`에서 경과 시간을 재고
`PoolManager.Instance.Release(gameObject)`를 호출한다. `Destroy`를 쓰면 풀의
`instanceToPrefab` 딕셔너리에 죽은 참조가 남는다.

- [ ] **Step 2: `git mv` 로 파일명 변경**

```bash
git mv Assets/Scripts/PlayerShooter.cs      Assets/Scripts/AutoAttack.cs
git mv Assets/Scripts/PlayerShooter.cs.meta Assets/Scripts/AutoAttack.cs.meta
```

`.meta`를 함께 옮기므로 GUID `518fc39c...`가 유지되고 TestScene:638의 참조가 살아남는다.
클래스명도 `AutoAttack`으로 바꾼다 (파일명과 일치해야 함).

- [ ] **Step 3: `AutoAttack.cs` 작성**

스페이스바 입력을 제거한다. `stats.fireRate` 쿨타임마다 `FindNearestMonster()`로
가장 가까운 적을 찾아 `stats.bulletCount`발을 부채꼴로 쏜다.

- [ ] **Step 4: 커밋**

---

## Task 8: PlayerHealth (학생 빈칸 4)

**Files:**
- Create: `Assets/Scripts/PlayerHealth.cs`

**Interfaces:**
- Consumes: `PlayerStats.maxHp`, `GameManager.Instance.GameOver()`
- Produces:
  - `PlayerHealth.TakeDamage(int damage)` → `void` (학생 빈칸 4)
  - `PlayerHealth.Heal(int amount)` → `void`
  - `PlayerHealth.CurrentHp` → `int`

- [ ] **Step 1: 작성**

무적 시간은 `Time.time < lastHitTime + invincibleTime` 패턴 — `AutoAttack`의 쿨타임과 동일한 형태.
`lastHitTime` 초기값은 `-999f`로 둔다. 0이면 게임 시작 직후 무적이 걸린다.

- [ ] **Step 2: 커밋**

---

## Task 9: ExpGem + PlayerLevel (학생 빈칸 5, 6)

**Files:**
- Create: `Assets/Scripts/ExpGem.cs`
- Create: `Assets/Scripts/PlayerLevel.cs`

**Interfaces:**
- Consumes: `PlayerStats.magnetRadius`, `PoolManager`, `UpgradeManager.ShowCards()`
- Produces:
  - `ExpGem.Init(int amount)` → `void`
  - `PlayerLevel.AddExp(int amount)` → `void` (학생 빈칸 6)

**주의:** `UpgradeManager`는 Task 10에서 만든다. Task 4·8·9·10까지 쓴 뒤 컴파일 검증한다.

- [ ] **Step 1: `ExpGem.cs` 작성** — 마그넷이 학생 빈칸 5.

`PlayerLevel.Instance`를 쓰지 않는다. 싱글톤은 2개로 제한하기로 했으므로
`other.GetComponent<PlayerLevel>()`로 가져온다.

- [ ] **Step 2: `PlayerLevel.cs` 작성** — `AddExp()`가 학생 빈칸 6.

레벨업 시 `currentExp -= expToNextLevel`로 초과분을 이월한다.
임계값은 `baseExpToLevel * pow(expGrowthRate, level-1)`.

- [ ] **Step 3: 커밋**

---

## Task 10: UpgradeData + UpgradeManager + UpgradeCardUI (학생 빈칸 7)

**Files:**
- Create: `Assets/Scripts/UpgradeData.cs`
- Create: `Assets/Scripts/UpgradeManager.cs`
- Create: `Assets/Scripts/UpgradeCardUI.cs`
- Test: `Assets/Tests/EditMode/UpgradeManagerTests.cs` (Task 0 통과 시)

**Interfaces:**
- Consumes: `PlayerStats`, `PlayerHealth.Heal(int)`
- Produces:
  - `enum UpgradeType { MoveSpeed, AttackDamage, AttackSpeed, MagnetRadius, MaxHealth, BulletCount }`
  - `UpgradeData` (ScriptableObject) — `upgradeName`, `description`, `icon`, `type`, `value`
  - `UpgradeManager.ShowCards()` → `void`
  - `UpgradeManager.ApplyUpgrade(UpgradeData)` → `void` (학생 빈칸 7)
  - `UpgradeCardUI.Show(UpgradeData, UpgradeManager)` → `void`

- [ ] **Step 1: 실패하는 테스트 작성** (Task 0 통과 시)

```csharp
using NUnit.Framework;
using UnityEngine;

public class UpgradeManagerTests
{
    [Test]
    public void ApplyUpgrade_AttackSpeed_NeverDropsToZeroOrBelow()
    {
        GameObject player = new GameObject("Player");
        PlayerStats stats = player.AddComponent<PlayerStats>();
        stats.fireRate = 0.1f;

        UpgradeManager manager = new GameObject("Mgr").AddComponent<UpgradeManager>();
        manager.SetTestDependencies(stats, null);

        UpgradeData upgrade = ScriptableObject.CreateInstance<UpgradeData>();
        upgrade.type  = UpgradeType.AttackSpeed;
        upgrade.value = 999f;   // 터무니없이 큰 값

        manager.ApplyUpgrade(upgrade);

        Assert.Greater(stats.fireRate, 0f, "fireRate가 0 이하가 되면 한 프레임에 총알이 무한히 나온다");

        Object.DestroyImmediate(player);
        Object.DestroyImmediate(manager.gameObject);
        Object.DestroyImmediate(upgrade);
    }

    [Test]
    public void ApplyUpgrade_MoveSpeed_AddsValue()
    {
        GameObject player = new GameObject("Player");
        PlayerStats stats = player.AddComponent<PlayerStats>();
        stats.moveSpeed = 5f;

        UpgradeManager manager = new GameObject("Mgr").AddComponent<UpgradeManager>();
        manager.SetTestDependencies(stats, null);

        UpgradeData upgrade = ScriptableObject.CreateInstance<UpgradeData>();
        upgrade.type  = UpgradeType.MoveSpeed;
        upgrade.value = 1.5f;

        manager.ApplyUpgrade(upgrade);

        Assert.AreEqual(6.5f, stats.moveSpeed, 0.001f);

        Object.DestroyImmediate(player);
        Object.DestroyImmediate(manager.gameObject);
        Object.DestroyImmediate(upgrade);
    }
}
```

`SetTestDependencies(PlayerStats, PlayerHealth)`를 `UpgradeManager`에 추가한다.
인스펙터 배선 없이 테스트하기 위한 것이고, 프로덕션 코드에서는 호출하지 않는다.
`cardPanel`이 null이어도 `ApplyUpgrade`가 죽지 않아야 한다 (null 가드 필수).

- [ ] **Step 2: 테스트 실패 확인**

- [ ] **Step 3: 세 파일 구현**

`ApplyUpgrade`의 `AttackSpeed`에 `Mathf.Max(0.05f, ...)` 하한을 반드시 넣는다.
`MaxHealth`는 최대 체력을 올리면서 그만큼 회복시킨다 (2줄).

- [ ] **Step 4: 테스트 통과 확인**

- [ ] **Step 5: 전체 컴파일 검증** — Task 4·8·9·10이 서로를 참조하므로 여기서 처음 컴파일된다.

- [ ] **Step 6: 커밋**

---

## Task 11: ShooterBehaviour + MonsterBullet

**Files:**
- Create: `Assets/Scripts/ShooterBehaviour.cs`
- Create: `Assets/Scripts/MonsterBullet.cs`

**Interfaces:**
- Consumes: `MonsterBehaviour`, `MonsterData.attackRange`, `PoolManager`, `PlayerHealth.TakeDamage(int)`
- Produces: `MonsterBullet.Initialize(Vector2 direction, int damage)` → `void`

- [ ] **Step 1: `MonsterBullet.cs` 작성** — `Bullet`과 대칭. 플레이어를 때린다.
- [ ] **Step 2: `ShooterBehaviour.cs` 작성** — `attackRange` 밖이면 접근, 안이면 멈춰서 발사.
- [ ] **Step 3: 컴파일 검증**
- [ ] **Step 4: 커밋**

---

## Task 12: ExploderBehaviour

**Files:**
- Create: `Assets/Scripts/ExploderBehaviour.cs`

**Interfaces:**
- Consumes: `MonsterBehaviour.OnMonsterDeath()`, `MonsterData.explosionRadius`, `Physics2D.OverlapCircleAll`

- [ ] **Step 1: 작성** — 추적은 `ChaseBehaviour`와 동일, `OnMonsterDeath()`에서 범위 데미지.
- [ ] **Step 2: 컴파일 검증**
- [ ] **Step 3: 커밋**

---

## Task 13: GameManager (생존 타이머)

**Files:**
- Modify: `Assets/Scripts/GameManager.cs` (전체 교체, GUID 유지)

**Interfaces:**
- Produces:
  - `GameManager.Instance` → `GameManager`
  - `GameManager.IsPlaying` → `bool`
  - `GameManager.SurvivedTime` → `float` (Task 6이 `appearTime` 판정에 쓴다)
  - `GameManager.GameOver()` → `void`
  - `GameManager.Restart()` → `void`

- [ ] **Step 1: 전체 교체**

- 점수 → 생존 시간(`MM:SS` 표기)
- **`Start()`와 `Restart()`에서 `Time.timeScale = 1f`로 되돌린다.**
  강화 카드가 뜬 상태(`timeScale = 0`)에서 재시작하면 게임이 멈춘 채로 시작한다.
- `GameOver()`는 이미 게임오버면 무시한다 (중복 호출 방지)
- 기존 코드의 `score += Mathf.RoundToInt(Time.deltaTime)`은
  **매 프레임 0으로 반올림돼 점수가 거의 오르지 않는 버그였다.** 타이머로 대체되며 해소된다.
- UI 참조는 전부 null 가드 (씬 배선 전에도 죽지 않아야 함)

- [ ] **Step 2: 컴파일 검증**
- [ ] **Step 3: 커밋**

---

## Task 14: CameraFollow + InfiniteBackground

**Files:**
- Create: `Assets/Scripts/CameraFollow.cs`
- Create: `Assets/Scripts/InfiniteBackground.cs`

- [ ] **Step 1: `CameraFollow.cs`** — `LateUpdate`에서 `Vector3.SmoothDamp`. z는 유지.
- [ ] **Step 2: `InfiniteBackground.cs`** — 큰 타일 스프라이트를 `tileSize` 격자에 스냅.
- [ ] **Step 3: 컴파일 검증**
- [ ] **Step 4: 커밋**

---

## Task 15: Ball 제거

**Files:**
- Delete: `Assets/Scripts/Ball.cs`, `Assets/Scripts/Ball.cs.meta`
- Delete: `Assets/Scripts/BallSpawner.cs`, `Assets/Scripts/BallSpawner.cs.meta`
- Modify: `Assets/Scenes/TestScene.unity` (BallSpawner 오브젝트 제거)

**주의:** `BallSpawner`는 TestScene:1772에 붙어 있다. 스크립트만 지우면 씬에
"Missing script" 구멍이 남는다. 씬 YAML을 손으로 편집하는 것은 위험하므로
**Task 16의 에디터 스크립트로 처리한다.**

- [ ] **Step 1: `Ball.prefab`은 남긴다** — 최근 커밋 이력이 Ball 작업이므로 파일은 보존하고
      씬에서만 뺀다 (설계 문서 4절).
- [ ] **Step 2: Task 16 이후에 스크립트 파일 삭제**

---

## Task 16: 에디터 셋업 스크립트 (에셋·프리팹·씬 자동 구성)

**Files:**
- Create: `Assets/Editor/VampireSurvivorsSetup.cs`
- Create: `Assets/Editor/Gwangmun.Game.Editor.asmdef` (Task 0 통과 시에만)

**Interfaces:**
- Produces: `VampireSurvivorsSetup.RunSetup()` — `-executeMethod`로 호출되는 static 메서드

**이유.** 씬 YAML을 손으로 편집하면 조용히 깨진다. Unity API로 씬을 구성하면
정확하고, 재실행 가능하고, 무엇을 만들었는지 코드로 남는다.

- [ ] **Step 1: 스크립트 작성**

수행할 일:
1. `Assets/Data/Monsters/`에 `MonsterData` 3개 생성 — 각 행동 1종씩, `appearTime`은
   Chase=0, Shooter=30, Exploder=60. **Chase는 반드시 `appearTime: 0`** (아니면 초반에 아무도 안 나온다)
2. `Assets/Data/Upgrades/`에 `UpgradeData` 6개 생성 — 6종 각 1개
3. 몬스터 프리팹 3종 생성 (`Monster_Chase`, `Monster_Shooter`, `Monster_Exploder`)
4. `ExpGem`, `MonsterBullet` 프리팹 생성
5. TestScene에서 BallSpawner 오브젝트 제거
6. `PoolManager`, `MonsterFactory` 오브젝트 생성 및 배선
7. 플레이어에 `PlayerStats`, `PlayerHealth`, `PlayerLevel`, `AutoAttack` 부착, 중력 0
8. UI 캔버스 구성 (타이머, 체력바, 경험치바, 강화 카드 3장, 게임오버 패널)
9. 씬 저장

- [ ] **Step 2: 실행**

```bash
"/c/Program Files/Unity/Hub/Editor/6000.3.12f1/Editor/Unity.exe" -batchmode -quit -nographics \
  -projectPath "$(pwd -W)" -executeMethod VampireSurvivorsSetup.RunSetup \
  -logFile "$SCRATCH/setup.log"
```

- [ ] **Step 3: 결과 확인** — 생성된 에셋 목록과 씬 변경을 로그로 확인.
- [ ] **Step 4: 커밋**

---

## Task 17: 최종 검증

- [ ] **Step 1: 전체 컴파일 검증** — `error CS` 0건.
- [ ] **Step 2: 전체 테스트 실행** — 전부 통과.
- [ ] **Step 3: PlayMode 스모크 테스트**

가능하면 PlayMode 테스트로 다음을 확인한다:
- 5초 재생 후 몬스터가 1마리 이상 스폰됐는가
- 총알이 자동으로 발사되는가

불가능하면 **"에디터에서 플레이 검증 안 됨"을 정직하게 보고한다.**
컴파일이 된다는 것과 게임이 재밌다는 것은 다른 문제다.

- [ ] **Step 4: 학생 배포본 추출 가능 여부 확인** — 빈칸 표식 7개가 전부 있는지 grep으로 확인.

```bash
grep -rn "학생 빈칸" Assets/Scripts/ | wc -l    # 기대: 14 (빈칸당 시작·끝 2줄)
```

- [ ] **Step 5: 최종 커밋**

---

## Self-Review 결과

**Spec coverage:**

| 설계 문서 항목 | 담당 태스크 |
|---|---|
| 탑다운 전환 (결정 1) | Task 1 |
| 메서드 단위 클론코딩 (결정 2) | 전 태스크 (빈칸 표식) |
| PlayerStats 단일 창고 (결정 3) | Task 1 |
| 싱글톤 2개 (결정 4) | Task 2 (PoolManager), Task 13 (GameManager) |
| 오브젝트 풀링 (결정 4) | Task 2 |
| 심플 팩토리 (결정 4) | Task 5 |
| 몬스터 행동 3종 (결정 5) | Task 3, 11, 12 |
| 학생 빈칸 7개 | Task 1(1), 3(2), 7(3), 8(4), 9(5,6), 10(7) |
| MonsterData SO | Task 3 |
| UpgradeData SO | Task 10 |
| 풀링 시연 토글 | Task 2 (`poolingEnabled`) |
| Ball 삭제 | Task 15, 16 |
| 카메라·배경 | Task 14 |
| 검증 (설계 9절) | Task 17 |

**미커버 항목:** 설계 9절의 "각 일차 종료 시점 스냅샷"은 강사가 수업 직전에 브랜치로
따야 하는 작업이라 이 계획에 넣지 않는다. 최종 보고에 후속 작업으로 명시한다.

**Type consistency 확인:**
- `MonsterController.TakeDamage(float)` vs `PlayerHealth.TakeDamage(int)` — **의도적으로 다르다.**
  몬스터는 `PlayerStats.damage`(float)를 받고, 플레이어는 `MonsterData.contactDamage`(int)를 받는다.
- `Bullet.Initialize(Vector2, float)` vs `MonsterBullet.Initialize(Vector2, int)` — 위와 같은 이유.
- `MonsterBehaviour.Init(MonsterData)`와 `MonsterController.Init(MonsterData)` — 이름이 같고
  둘 다 존재한다. `MonsterController.Init`이 `MonsterBehaviour.Init`을 호출하는 관계다.
