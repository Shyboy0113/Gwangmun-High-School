# 뱀서라이크 전환 설계

작성일: 2026-07-17
대상: 광문고 파견 학생 14명 대상 3일 유니티 수업

## 1. 배경

현재 프로젝트는 횡스크롤 플랫포머 프로토타입이다. 이를 뱀서라이크(Vampire Survivors-like) 장르로 전환하되,
대학생 동아리원이 고등학생 14명을 상대로 3일간 진행하는 수업 자료로 쓸 수 있는 형태여야 한다.

### 수업 조건

| 항목 | 값 |
|---|---|
| 학생 수 | 14명 |
| 기간 | 3일, 하루 2~3시간 (총 6~9시간) |
| 학생 수준 | 파이썬 등 기초 프로그래밍 수업 경험 있음. C#·유니티는 처음 |
| 진행 방식 | 강사 배포본을 받아 메서드 단위 클론코딩 |
| 최종 활동 | ScriptableObject로 자기 몬스터 만들기 |

### 현재 프로젝트 상태

스크립트 8개로 구성된 횡스크롤 프로토타입이다.

- `PlayerController` — 좌우 이동, 중력 기반 점프(`isGrounded`, Ground 태그)
- `PlayerShooter` — 스페이스바 수동 발사, 바라보는 방향으로 총알
- `Bullet` — 직선 이동, Monster 태그와 충돌 시 점수 +10
- `MonsterController` — X축으로만 플레이어 추적, 체력 3 하드코딩
- `MonsterSpawner` — 좌우 양끝에서 스폰, 시간에 따라 스폰 간격 감소
- `GameManager` — 싱글톤, 생존 시간 점수, 게임오버/재시작
- `Ball` / `BallSpawner` — 위에서 낙하하는 장애물. Ground 충돌 시 소멸

레벨업, 경험치, 강화 개념은 전혀 없다. 플레이어가 강해지는 수단이 하나도 없는 상태다.

## 2. 목표와 비목표

### 목표

1. 뱀서라이크의 장르 정체성 4가지를 모두 구현한다 — 자동공격, 경험치 기반 레벨업 3택1 강화,
   사방에서 몰려드는 적 떼, 접촉 데미지 기반 체력 시스템
2. 학생이 그 4가지를 모두 자기 손으로 타이핑한 경험을 갖게 한다
3. 코딩 없이 ScriptableObject 에셋 조작만으로 새 몬스터를 만들 수 있게 한다

### 비목표 (이번 범위에서 제외)

- 무기 여러 종류 동시 운용 (뱀서라이크의 주요 요소지만 6~9시간에 안 들어감)
- 무기 진화, 상자, 골드, 메타 프로그레션, 캐릭터 선택, 부활
- 오브젝트 풀링 (이 규모에서 체감 없음. 성능 문제 발생 시에만 도입)
- 보스 몬스터 (`appearTime`으로 흉내는 가능하나 전용 로직은 없음)

## 3. 핵심 결정 사항

### 결정 1: 탑다운으로 전환한다

**용어 정리.** 셋을 혼동하기 쉬우므로 먼저 구분한다.

| 용어 | 뜻 | 예시 |
|---|---|---|
| 횡스크롤(측면뷰) | 옆에서 본 시점. 중력이 아래로 작용, 점프 있음 | 슈퍼마리오. **현재 프로젝트가 이 상태** |
| 쿼터뷰(아이소메트릭) | 45도 비스듬히 위에서. 마름모 타일, 좌표계 회전 | 디아블로 |
| 탑다운 | 거의 바로 위에서. 정사각 타일 | **이번에 채택** |

**뱀서라이크 원작은 쿼터뷰가 아니다.** 정사각 타일 위에서 바로 내려다보는 탑다운이고,
캐릭터 스프라이트만 정면을 보게 그려져 있어 쿼터뷰처럼 보일 뿐이다. 좌표 변환도 마름모 타일도 없다.
한국에서 이를 뭉뚱그려 쿼터뷰라 부르는 경우가 많아 혼동이 생긴다. 이번 프로젝트도 원작과 같은 방식을 쓴다.

아이소메트릭 쿼터뷰를 택하지 않은 이유는, 마름모 타일맵과 Y좌표 기반 앞뒤 정렬에 더해
"위쪽 방향키를 눌렀는데 화면에서는 대각선으로 간다"는 좌표계 불일치를 학생에게 설명해야 하기 때문이다.
1일차 `Move()` 8줄이 감당할 수 없는 양이고, Grass 타일 에셋도 못 쓰게 된다.

**이유.** 뱀서라이크의 핵심 압박감은 사방이 적으로 둘러싸이는 데서 나온다. 횡스크롤에서는 적이 좌우에서만
오므로 이 감각이 성립하지 않는다.

**영향.** 중력·점프·`isGrounded`·Ground 태그를 전부 제거한다. Rigidbody2D의 gravityScale은 0으로 한다.
버리는 코드는 `PlayerController` 47줄과 `MonsterController` 39줄뿐이라 부담이 작다.
Grass 계열 타일맵 에셋은 바닥 텍스처로 재활용한다.

### 결정 2: 클론코딩 단위는 스크립트가 아니라 메서드다

**이유.** 총 6~9시간 중 1일차 상당 부분이 유니티 에디터·C# 문법 입문에 쓰이고, 14명 규모에서는
설치·컴파일 에러 트러블슈팅 시간이 반드시 발생한다. 실제 기능 구현 가능 시간은 4~6시간으로 봐야 한다.
백지에서 시작하면 핵심 4가지 중 2개도 못 끝낸다.

**방침.** 배포본에 클래스 뼈대·인스펙터 연결·UI·프리팹을 전부 완성해 둔다. 학생은 주석으로 표시된
핵심 메서드 본문만 채운다. 선별 기준은 **결과가 화면에 즉시 보이는가**다.
컴포넌트 캐싱, 인스펙터 배선 같은 배워도 남지 않는 작업은 미리 처리한다.

### 결정 3: PlayerStats를 수치의 단일 창고로 둔다

이동속도·공격력·공속·마그넷 반경·최대체력·발사 수를 한 곳에 모은다.
강화 카드는 "이 창고의 숫자를 올리는 것"으로 단순해지고, `PlayerController`·`AutoAttack`·`ExpGem`은
이 창고를 읽기만 한다. 이 중간층이 없으면 강화 카드가 여러 스크립트를 직접 건드려야 해서
고등학생에게 설명이 불가능해진다.

## 4. 스크립트 인벤토리

### 기존 8개의 처리

| 파일 | 처리 | 내용 |
|---|---|---|
| `Bullet.cs` | 거의 유지 | `Initialize(direction)` 구조를 자동공격이 그대로 쓴다. 점수 부여(29줄)를 제거하고 `PlayerStats.damage`를 몬스터에 전달만 한다. 젬 드롭은 총알이 아니라 몬스터 사망 시점의 책임이다 |
| `MonsterSpawner.cs` | 골격 유지 | 스폰 위치를 좌우 2곳에서 카메라 밖 원형 둘레로 변경. 난이도 상승 로직은 재활용 |
| `GameManager.cs` | 골격 유지 | 싱글톤·`IsPlaying`·게임오버·재시작 유지. 점수를 생존 타이머로 변경 |
| `PlayerController.cs` | 새로 씀 | 중력·점프·`isGrounded` 제거. 8방향 이동 |
| `MonsterController.cs` | 새로 씀 | 정규화된 8방향 추적. 체력·속도를 `MonsterData`에서 읽음. 사망 시 젬 드롭, 플레이어 접촉 시 `PlayerHealth.TakeDamage()` 호출 |
| `PlayerShooter.cs` | `AutoAttack.cs`로 개명 | 스페이스바 발사 → 자동 조준 발사 |
| `Ball.cs` | 삭제 | 낙하와 Ground 충돌이 전제라 탑다운에 성립 불가 |
| `BallSpawner.cs` | 삭제 | 위와 같음 |

`Ball.prefab`과 Ball 태그도 정리 대상이나, 최근 커밋 이력이 Ball 작업이므로 파일은 남기고
씬에서만 제외한다.

### 신규 9개

**학생이 손대는 것 (4개)**

- `PlayerHealth.cs` — 체력, 무적 시간, 사망 처리
- `ExpGem.cs` — 경험치 젬, 마그넷 흡수
- `PlayerLevel.cs` — 경험치 누적, 레벨업 발동
- `UpgradeManager.cs` — 3택1 카드 뽑기(배포본) + 강화 적용(학생)

**학생이 열어볼 일 없는 인프라 (5개)**

- `PlayerStats.cs` — 공용 수치 창고
- `CameraFollow.cs` — 플레이어 추적
- `InfiniteBackground.cs` — 배경 타일 반복
- `MonsterData.cs` — ScriptableObject 정의
- `UpgradeData.cs` — ScriptableObject 정의

## 5. 게임 루프

```
몬스터 사망
  → 경험치 젬 드롭
  → 마그넷 반경 진입 시 플레이어에게 흡수
  → PlayerLevel.AddExp()
  → 경험치가 임계값 초과
  → Time.timeScale = 0 (게임 정지)
  → 강화 카드 3장 표시 (중복 없이 랜덤)
  → 학생이 하나 클릭
  → UpgradeManager.ApplyUpgrade() → PlayerStats 수치 상승
  → Time.timeScale = 1 (재개)
```

### 책임 경계

각 스크립트가 어디까지 아는지를 명확히 해둔다. 이 경계가 흐려지면 학생에게 설명이 불가능해진다.

| 주체 | 책임 | 모르는 것 |
|---|---|---|
| `Bullet` | `PlayerStats.damage`를 몬스터에 전달하고 소멸 | 몬스터가 죽는지, 젬이 뭔지 |
| `MonsterController` | 추적, 피격 시 체력 감소, 사망 시 젬 드롭, 접촉 시 `PlayerHealth.TakeDamage()` 호출 | 경험치가 얼마나 쌓였는지, 레벨업이 뭔지 |
| `ExpGem` | 마그넷 흡수, 획득 시 `PlayerLevel.AddExp()` 호출 | 레벨업 조건 |
| `PlayerLevel` | 경험치 누적, 임계값 초과 시 `UpgradeManager` 호출 | 강화 종류 |
| `UpgradeManager` | 카드 3장 뽑기·표시, 선택 결과를 `PlayerStats`에 반영 | 경험치가 어디서 왔는지 |
| `PlayerStats` | 수치 보관만 | 나머지 전부 |

**데미지 흐름.** 총알 → 몬스터(피격) 방향과 몬스터 → 플레이어(접촉) 방향 두 갈래다.
전자는 `Bullet`이 `MonsterController`에, 후자는 `MonsterController`가 `PlayerHealth`에 전달한다.
접촉 데미지의 주체가 몬스터인 이유는 데미지 값이 `MonsterData.contactDamage`에 있기 때문이다.

**사망 처리.** `PlayerHealth.TakeDamage()`에서 체력이 0 이하가 되면
기존 `GameManager.GameOver()`를 호출한다.

## 6. 데이터 설계

### MonsterData

몬스터 프리팹은 `Monster.prefab` 하나뿐이다. 데이터를 주입해서 서로 다른 몬스터가 나온다.
학생은 새 몬스터를 만들 때 프리팹을 복제하지 않고 **에셋 파일 하나만 복제**한다.

```csharp
[CreateAssetMenu(menuName = "뱀서라이크/몬스터 데이터")]
public class MonsterData : ScriptableObject
{
    [Header("겉모습")]
    public string monsterName = "새 몬스터";
    public Sprite sprite;
    public Color  color = Color.white;
    public float  size  = 1f;

    [Header("능력치")]
    public int   maxHp         = 3;
    public float moveSpeed     = 2f;
    public int   contactDamage = 1;

    [Header("보상")]
    public int expAmount = 1;

    [Header("등장 조건")]
    public float appearTime = 0f;   // 이 시간(초)이 지나야 등장
}
```

주입은 `MonsterController.Init()`(배포본)이 담당한다. 데이터를 받아 스프라이트·색·크기·체력·속도를
자기 몸에 반영하는 5줄짜리다. `MonsterSpawner`는 `List<MonsterData>`를 들고 `appearTime`이 지난
것들 중에서 랜덤으로 뽑는다.

**`color`와 `size`를 넣은 이유.** 스프라이트를 직접 그릴 시간이 없는 학생도 색과 크기만 바꿔
즉시 자기 몬스터를 만들 수 있다. 그림 실력 없이도 3일차 활동이 성립한다.

**`appearTime`을 넣은 이유.** "내 보스는 3분 뒤에 등장"을 학생이 직접 정할 수 있다.

**에셋 공유.** 완성된 몬스터는 파일 하나라 학생끼리 주고받을 수 있다. 14명이 만든 걸 모아
한 게임에 다 넣고 같이 돌려보는 것이 3일차 마지막 활동이다.

### UpgradeData

```csharp
public enum UpgradeType
{
    MoveSpeed, AttackDamage, AttackSpeed,
    MagnetRadius, MaxHealth, BulletCount,
}

[CreateAssetMenu(menuName = "뱀서라이크/강화 데이터")]
public class UpgradeData : ScriptableObject
{
    public string upgradeName;
    [TextArea] public string description;
    public Sprite icon;
    public UpgradeType type;
    public float value;
}
```

**6종인 이유.** 5종 이하면 3택1이 매번 같은 카드만 나와 뽑는 재미가 없다. 6종이 최소선이다.
`BulletCount`(부채꼴로 여러 발)는 빌드가 세지는 체감이 가장 확실해서 포함했다.
여러 발 발사 처리 자체는 `AutoAttack`(배포본)이 담당한다.

### PlayerStats

```csharp
public class PlayerStats : MonoBehaviour
{
    public float moveSpeed    = 5f;
    public float damage       = 1f;
    public float fireRate     = 0.3f;
    public float magnetRadius = 2f;
    public int   maxHp        = 10;
    public int   bulletCount  = 1;
}
```

## 7. 커리큘럼 매핑

학생이 타이핑하는 빈칸은 총 7개다.

### 1일차 (2~3시간) — 유니티·C# 입문 + 움직임

에디터 구경, 프로젝트 열기, 씬 구조 설명에 대부분을 쓴다. 코드는 2개, 합쳐서 14줄.

| 빈칸 | 줄 수 | 가르치는 개념 |
|---|---|---|
| `PlayerController.Move()` | 8 | 벡터, `normalized`. 대각선이 왜 더 빠른지, 왜 정규화하는지 |
| `MonsterController.Chase()` | 6 | `(목표 위치 - 내 위치).normalized`. 방금 배운 벡터의 즉시 재활용 |

1일차 종료 시점에 몬스터가 쫓아오는 것이 화면에 보인다.

### 2일차 (2~3시간) — 게임이 되는 날

| 빈칸 | 줄 수 | 가르치는 개념 |
|---|---|---|
| `AutoAttack.FindNearestMonster()` | 18 | 배열 순회로 최솟값 찾기 |
| `PlayerHealth.TakeDamage()` | 12 | 쿨타임 패턴 (무적 시간) |

`FindNearestMonster()`가 이 수업의 하이라이트다.

```csharp
Transform FindNearestMonster()
{
    GameObject[] monsters = GameObject.FindGameObjectsWithTag("Monster");
    Transform nearest = null;
    float minDist = Mathf.Infinity;

    for (int i = 0; i < monsters.Length; i++)
    {
        float dist = Vector2.Distance(transform.position, monsters[i].transform.position);
        if (dist < minDist)
        {
            minDist = dist;
            nearest = monsters[i].transform;
        }
    }
    return nearest;
}
```

배열을 돌며 최솟값을 찾는 것은 파이썬 수업에서 반드시 해본 문제다. 문법만 다르고 논리는 같다.
그리고 이 18줄을 치는 순간 장르가 바뀐다 — 스페이스바를 놓아도 총알이 알아서 나간다.

`TakeDamage()`의 무적 시간은 `Time.time >= lastHitTime + invincibleTime` 패턴으로,
자동공격 쿨타임에서 방금 본 것과 동일하다. 기존 `PlayerShooter.cs:21`에 이미 있는 형태라
배포본 전체가 일관된다. 같은 패턴이 두 곳에 쓰이는 것을 보는 학습 효과가 크다.

2일차 종료 시점에 죽을 수 있는 게임이 된다.

### 3일차 (2~3시간) — 성장 루프 + 내 몬스터 만들기

| 빈칸 | 줄 수 | 가르치는 개념 |
|---|---|---|
| `ExpGem` 마그넷 | 10 | `Vector2.MoveTowards`, 거리 판정 |
| `PlayerLevel.AddExp()` | 6 | 누적과 임계값 |
| `UpgradeManager.ApplyUpgrade()` | 12 | `switch`문, `Time.timeScale` |

`ApplyUpgrade()`는 다음과 같다.

```csharp
public void ApplyUpgrade(UpgradeData upgrade)
{
    switch (upgrade.type)
    {
        case UpgradeType.MoveSpeed:    stats.moveSpeed    += upgrade.value; break;
        case UpgradeType.AttackDamage: stats.damage       += upgrade.value; break;
        case UpgradeType.AttackSpeed:  stats.fireRate      = Mathf.Max(0.05f, stats.fireRate - upgrade.value); break;
        case UpgradeType.MagnetRadius: stats.magnetRadius += upgrade.value; break;
        case UpgradeType.MaxHealth:    stats.maxHp        += (int)upgrade.value; break;
        case UpgradeType.BulletCount:  stats.bulletCount  += (int)upgrade.value; break;
    }
    Time.timeScale = 1f;
}
```

`AttackSpeed`에만 `Mathf.Max`가 붙는다. 공속 강화는 쿨타임을 빼는 것이라 0 이하가 되면
한 프레임에 총알이 무한히 나오면서 게임이 얼어붙는다. 수업 중 실제로 터질 버그라 처음부터 막아뒀다.
"왜 여기만 다르지?"라는 질문이 나오면 좋은 5분짜리 설명거리다.

이후 코딩 없이 `MonsterData` 에셋을 복제해 자기 몬스터를 만들고, 서로 교환해 플레이한다.

### 난이도 편차 대응

강화 카드를 **추가**하는 것은 코딩이 필요 없다 — `UpgradeData` 에셋을 만들어 리스트에 넣으면 된다.
반면 **새로운 종류**의 강화(체력 재생, 관통 등)는 `UpgradeType` enum과 `switch`를 수정해야 한다.
빨리 끝낸 학생에게 줄 심화 과제가 여기서 나온다.

## 8. 의도적으로 택한 비효율

`FindGameObjectsWithTag`는 매번 씬 전체를 훑는 느린 방식이다. 실무라면 스포너가 몬스터 리스트를
들고 있게 한다. 다만 발사 시점에만(초당 3회 정도) 호출되고 이 규모에서는 체감이 없다.
**배열 순회로 최솟값 찾기라는 교보재 가치가 최적화보다 크다고 판단해 가르치기 쉬운 쪽을 택했다.**

## 9. 검증 방법

수업 자료이므로 자동화 테스트보다 배포본이 실제로 돌아가는지가 중요하다.

1. **빈칸 채운 상태로 전체 플레이** — 3일차 종료 상태를 강사가 먼저 끝까지 플레이해 본다.
   레벨업이 최소 5회 이상 발생하고, 6종 강화가 모두 최소 1회씩 뽑히는지 확인한다.
2. **빈칸 비운 상태로 컴파일** — 학생 배포본이 빈 메서드 상태에서도 컴파일 에러 없이
   에디터가 열리는지 확인한다. 이게 깨지면 1일차가 통째로 날아간다.
3. **각 일차 종료 시점 스냅샷** — 1·2·3일차 각각의 완성 상태를 브랜치나 폴더로 보관한다.
   뒤처진 학생을 다음 날 아침에 따라잡게 하려면 반드시 필요하다.
4. **몬스터 수 부하 확인** — 스폰 간격이 최소치에 도달한 후반부에 프레임이 유지되는지 확인한다.
   학교 노트북 사양에서 확인해야 의미가 있다.

## 10. 열린 사항

- 강화 카드 6종의 구체적 수치(`value`)는 강사가 플레이 테스트 후 결정한다.
- 경험치 임계값 곡선(레벨당 필요 경험치 증가율)도 마찬가지다.
- 학교 노트북 사양이 확인되면 동시 몬스터 수 상한을 정한다.
