using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// 강사용 정답본의 데이터 에셋과 프리팹을 코드로 생성한다.
/// 씬 YAML을 손으로 편집하면 조용히 깨지므로 Unity API로 만든다.
/// 재실행해도 안전하도록 매번 덮어쓴다.
///
/// 실행: 메뉴 [뱀서라이크/1. 에셋·프리팹 생성] 또는
///   -executeMethod VS_AssetSetup.RunSetup
/// </summary>
public static class VS_AssetSetup
{
    const string DataMonstersDir = "Assets/Data/Monsters";
    const string DataUpgradesDir = "Assets/Data/Upgrades";
    const string PrefabDir       = "Assets/Prefabs";

    const string MonsterSpriteGuid = "8901a79ef817b2649b877d6b3bb5370f"; // 기존 Monster 프리팹의 스프라이트
    const string MonsterBasePrefab = "Assets/Monster.prefab";

    [MenuItem("뱀서라이크/1. 에셋·프리팹 생성")]
    public static void RunSetup()
    {
        Debug.Log("[VS_AssetSetup] 시작");

        EnsureFolder("Assets/Data");
        EnsureFolder(DataMonstersDir);
        EnsureFolder(DataUpgradesDir);
        EnsureFolder(PrefabDir);

        Sprite sprite = LoadSprite(MonsterSpriteGuid);
        if (sprite == null)
            Debug.LogWarning("[VS_AssetSetup] 몬스터 스프라이트를 못 찾음. 색만으로 구분됨.");

        // 순서 중요: 프리팹이 서로를 참조하므로 젬·몬스터총알을 먼저 만든다.
        GameObject expGemPrefab      = CreateExpGemPrefab(sprite);
        GameObject monsterBulletPrefab = CreateMonsterBulletPrefab(sprite);

        CreateMonsterPrefab("Monster_Chase",    MonsterBehaviorType.Chase,    expGemPrefab, null);
        CreateMonsterPrefab("Monster_Shooter",  MonsterBehaviorType.Shooter,  expGemPrefab, monsterBulletPrefab);
        CreateMonsterPrefab("Monster_Exploder", MonsterBehaviorType.Exploder, expGemPrefab, null);
        CreateMonsterPrefab("Monster_Boss",     MonsterBehaviorType.Boss,     expGemPrefab, monsterBulletPrefab);

        CreateMonsterData("Monster_Chase_Data",    "돌진하는 슬라임", MonsterBehaviorType.Chase,
                          new Color(0.4f, 0.9f, 0.4f), 1f, 3, 2.5f, 1, 1, 0f);
        CreateMonsterData("Monster_Shooter_Data",  "쏘는 눈알",     MonsterBehaviorType.Shooter,
                          new Color(0.5f, 0.7f, 1f), 0.9f, 4, 1.5f, 1, 2, 30f);
        CreateMonsterData("Monster_Exploder_Data", "터지는 폭탄",   MonsterBehaviorType.Exploder,
                          new Color(1f, 0.5f, 0.3f), 1.2f, 5, 2f, 2, 3, 60f);

        // 스피드형 — 전용 프리팹 없이 Chase 행동에 이동 속도만 크게. 저체력·소형·초반 등장.
        CreateMonsterData("Monster_Speed_Data",    "질주하는 유령", MonsterBehaviorType.Chase,
                          new Color(1f, 1f, 0.4f), 0.75f, 1, 5.5f, 1, 1, 15f);

        // 보스 — 복합형. 크고 튼튼하고 느리게 추적하며 탄막을 쏜다. 처치 시 게임 클리어.
        // appearTime 은 무시된다(스포너의 bossSpawnTime 으로 등장). isBoss = true.
        CreateMonsterData("Monster_Boss_Data",     "심연의 군주",   MonsterBehaviorType.Boss,
                          new Color(0.6f, 0.2f, 0.8f), 3f, 200, 1.3f, 3, 30, 0f, isBoss: true);

        CreateUpgrade("Up_MoveSpeed",   "날쌘 발",     "이동 속도 +1",        UpgradeType.MoveSpeed,   1f);
        CreateUpgrade("Up_Damage",      "날카로운 탄", "공격력 +1",           UpgradeType.AttackDamage, 1f);
        CreateUpgrade("Up_AttackSpeed", "빠른 손",     "공격 간격 -0.05초",   UpgradeType.AttackSpeed,  0.05f);
        CreateUpgrade("Up_Magnet",      "자석",        "젬 수집 범위 +1",     UpgradeType.MagnetRadius, 1f);
        CreateUpgrade("Up_MaxHealth",   "튼튼한 몸",   "최대 체력 +3",        UpgradeType.MaxHealth,    3f);
        CreateUpgrade("Up_BulletCount", "분열탄",      "총알 +1발",           UpgradeType.BulletCount,  1f);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[VS_AssetSetup] 완료");
    }

    static GameObject CreateMonsterPrefab(string name, MonsterBehaviorType type,
                                          GameObject expGemPrefab, GameObject monsterBulletPrefab)
    {
        // 기존 Monster.prefab을 베이스로 로드해서 스프라이트·머티리얼을 물려받는다.
        GameObject baseObj = PrefabUtility.LoadPrefabContents(MonsterBasePrefab);

        baseObj.name = name;
        baseObj.tag  = "Monster";   // 기존 프리팹은 Enemy 태그였다. 반드시 교체.

        // 콜라이더를 트리거로. 비트리거면 Kinematic 몬스터가 플레이어를 밀어낸다.
        BoxCollider2D box = baseObj.GetComponent<BoxCollider2D>();
        if (box != null) box.isTrigger = true;

        // 중력 제거 (탑다운).
        Rigidbody2D rb = baseObj.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        // 행동 컴포넌트를 정확히 하나 붙인다.
        RemoveIfPresent<ChaseBehaviour>(baseObj);
        RemoveIfPresent<ShooterBehaviour>(baseObj);
        RemoveIfPresent<ExploderBehaviour>(baseObj);
        RemoveIfPresent<BossBehaviour>(baseObj);

        MonsterBehaviour behaviour = null;
        switch (type)
        {
            case MonsterBehaviorType.Shooter:  behaviour = baseObj.AddComponent<ShooterBehaviour>(); break;
            case MonsterBehaviorType.Exploder: behaviour = baseObj.AddComponent<ExploderBehaviour>(); break;
            case MonsterBehaviorType.Boss:     behaviour = baseObj.AddComponent<BossBehaviour>(); break;
            default:                           behaviour = baseObj.AddComponent<ChaseBehaviour>(); break;
        }

        // MonsterController.expGemPrefab 배선
        MonsterController controller = baseObj.GetComponent<MonsterController>();
        if (controller != null)
            SetPrivateObjectField(controller, "expGemPrefab", expGemPrefab);

        // 사격형·보스: monsterBulletPrefab 배선 (둘 다 탄막을 쏜다)
        if ((type == MonsterBehaviorType.Shooter || type == MonsterBehaviorType.Boss) && monsterBulletPrefab != null)
            SetPrivateObjectField(behaviour, "monsterBulletPrefab", monsterBulletPrefab);

        string path = $"{PrefabDir}/{name}.prefab";
        GameObject saved = PrefabUtility.SaveAsPrefabAsset(baseObj, path);
        PrefabUtility.UnloadPrefabContents(baseObj);

        Debug.Log($"[VS_AssetSetup] 프리팹 생성: {path}");
        return saved;
    }

    static GameObject CreateExpGemPrefab(Sprite sprite)
    {
        GameObject obj = new GameObject("ExpGem");
        obj.tag = "Untagged";

        SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color  = new Color(0.4f, 1f, 1f);   // 청록색 젬
        sr.sortingOrder = 5;

        obj.transform.localScale = Vector3.one * 0.4f;

        CircleCollider2D col = obj.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 1.2f;   // 스케일 0.4 → 실제 약 0.48. 줍기 판정용

        obj.AddComponent<ExpGem>();

        string path = $"{PrefabDir}/ExpGem.prefab";
        GameObject saved = PrefabUtility.SaveAsPrefabAsset(obj, path);
        Object.DestroyImmediate(obj);

        Debug.Log($"[VS_AssetSetup] 프리팹 생성: {path}");
        return saved;
    }

    static GameObject CreateMonsterBulletPrefab(Sprite sprite)
    {
        GameObject obj = new GameObject("MonsterBullet");
        obj.tag = "Untagged";

        SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color  = new Color(1f, 0.3f, 0.6f);   // 분홍 탄
        sr.sortingOrder = 4;

        obj.transform.localScale = Vector3.one * 0.3f;

        Rigidbody2D rb = obj.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;

        CircleCollider2D col = obj.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.5f;

        obj.AddComponent<MonsterBullet>();

        string path = $"{PrefabDir}/MonsterBullet.prefab";
        GameObject saved = PrefabUtility.SaveAsPrefabAsset(obj, path);
        Object.DestroyImmediate(obj);

        Debug.Log($"[VS_AssetSetup] 프리팹 생성: {path}");
        return saved;
    }

    static void CreateMonsterData(string fileName, string monsterName, MonsterBehaviorType type,
                                  Color color, float size, int hp, float speed,
                                  int contactDamage, int exp, float appearTime, bool isBoss = false)
    {
        MonsterData data = ScriptableObject.CreateInstance<MonsterData>();
        data.monsterName   = monsterName;
        data.sprite        = LoadSprite(MonsterSpriteGuid);
        data.color         = color;
        data.size          = size;
        data.behaviorType  = type;
        data.maxHp         = hp;
        data.moveSpeed     = speed;
        data.contactDamage = contactDamage;
        data.expAmount     = exp;
        data.appearTime    = appearTime;
        data.isBoss        = isBoss;
        if (type == MonsterBehaviorType.Shooter)  data.attackRange     = 5f;
        if (type == MonsterBehaviorType.Exploder) data.explosionRadius = 2.5f;

        string path = $"{DataMonstersDir}/{fileName}.asset";
        AssetDatabase.DeleteAsset(path);
        AssetDatabase.CreateAsset(data, path);
        Debug.Log($"[VS_AssetSetup] 몬스터 데이터: {path}");
    }

    static void CreateUpgrade(string fileName, string name, string desc, UpgradeType type, float value)
    {
        UpgradeData data = ScriptableObject.CreateInstance<UpgradeData>();
        data.upgradeName = name;
        data.description = desc;
        data.type        = type;
        data.value       = value;

        string path = $"{DataUpgradesDir}/{fileName}.asset";
        AssetDatabase.DeleteAsset(path);
        AssetDatabase.CreateAsset(data, path);
        Debug.Log($"[VS_AssetSetup] 강화 데이터: {path}");
    }

    // ── 유틸 ──

    static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        string parent = Path.GetDirectoryName(path).Replace("\\", "/");
        string leaf   = Path.GetFileName(path);
        AssetDatabase.CreateFolder(parent, leaf);
    }

    static Sprite LoadSprite(string guid)
    {
        string path = AssetDatabase.GUIDToAssetPath(guid);
        if (string.IsNullOrEmpty(path)) return null;
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    static void RemoveIfPresent<T>(GameObject obj) where T : Component
    {
        T comp = obj.GetComponent<T>();
        if (comp != null) Object.DestroyImmediate(comp, true);
    }

    /// private [SerializeField] 참조를 SerializedObject로 안전하게 설정한다.
    static void SetPrivateObjectField(Object target, string fieldName, Object value)
    {
        SerializedObject so = new SerializedObject(target);
        SerializedProperty prop = so.FindProperty(fieldName);
        if (prop == null)
        {
            Debug.LogWarning($"[VS_AssetSetup] 필드 못 찾음: {target.GetType().Name}.{fieldName}");
            return;
        }
        prop.objectReferenceValue = value;
        so.ApplyModifiedPropertiesWithoutUndo();
    }
}
