using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// TestScene을 뱀서라이크로 배선한다. 컴포넌트를 붙이고 참조를 연결하고 UI를 만든다.
/// 씬 YAML을 손으로 고치면 조용히 깨지므로 Unity API로 처리한다.
///
/// 실행: 메뉴 [뱀서라이크/2. 씬 배선] 또는
///   -executeMethod VS_SceneSetup.RunSetup
///
/// 재실행 안전: 이미 붙은 컴포넌트·오브젝트는 찾아서 재사용한다.
/// </summary>
public static class VS_SceneSetup
{
    const string ScenePath   = "Assets/Scenes/TestScene.unity";
    const string SceneGuid   = "8c9cfa26abfee488c85f1582747f6a02";
    const string PrefabDir   = "Assets/Prefabs";
    const string MonsterDataDir = "Assets/Data/Monsters";
    const string UpgradeDataDir = "Assets/Data/Upgrades";

    // 기존 씬에서 재활용할 UI 오브젝트의 fileID (GlobalObjectId로 집는다)
    const long TimerTextGoId     = 91696440;   // 옛 점수 텍스트 → 생존 타이머
    const long GameOverPanelGoId = 1610375044; // 게임오버 패널
    const long FinalTextGoId     = 1952855943; // 옛 최종점수 텍스트 → 최종 생존시간

    [MenuItem("뱀서라이크/2. 씬 배선")]
    public static void RunSetup()
    {
        Debug.Log("[VS_SceneSetup] 시작");

        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        GameObject player  = GameObject.FindWithTag("Player");
        GameObject canvas  = Object.FindFirstObjectByType<Canvas>()?.gameObject;
        Camera mainCam     = Camera.main;
        GameObject gmObj   = Object.FindFirstObjectByType<GameManager>()?.gameObject;
        GameObject spawnerObj = Object.FindFirstObjectByType<MonsterSpawner>()?.gameObject;

        if (player == null || canvas == null || gmObj == null || spawnerObj == null)
        {
            Debug.LogError($"[VS_SceneSetup] 필수 오브젝트 누락: player={player}, canvas={canvas}, gm={gmObj}, spawner={spawnerObj}");
            return;
        }

        // 재활용 UI 집기
        GameObject timerGo   = ResolveSceneObject(TimerTextGoId);
        GameObject gameOver  = ResolveSceneObject(GameOverPanelGoId);
        GameObject finalGo   = ResolveSceneObject(FinalTextGoId);

        TMP_FontAsset font = TMP_Settings.defaultFontAsset;
        Sprite uiSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

        // ── 1. 인프라 오브젝트 ──
        PoolManager pool       = FindOrCreate<PoolManager>("PoolManager");
        MonsterFactory factory = FindOrCreate<MonsterFactory>("MonsterFactory");
        UpgradeManager upgrade = FindOrCreate<UpgradeManager>("UpgradeManager");

        // ── 2. 플레이어 컴포넌트 ──
        PlayerStats stats  = EnsureComponent<PlayerStats>(player);
        PlayerHealth health = EnsureComponent<PlayerHealth>(player);
        PlayerLevel level  = EnsureComponent<PlayerLevel>(player);

        // ── 3. 팩토리 배선 ──
        SetRef(factory, "chasePrefab",    LoadPrefab("Monster_Chase"));
        SetRef(factory, "shooterPrefab",  LoadPrefab("Monster_Shooter"));
        SetRef(factory, "exploderPrefab", LoadPrefab("Monster_Exploder"));

        // ── 4. 스포너 배선 ──
        SetRef(spawnerObj.GetComponent<MonsterSpawner>(), "factory", factory);
        SetRefList(spawnerObj.GetComponent<MonsterSpawner>(), "monsterTable", LoadAllMonsterData());

        // ── 5. UI 생성 ──
        // 체력바 (좌상단)
        Image healthFill = CreateBar(canvas.transform, "HealthBar", uiSprite,
            new Vector2(0f, 1f), new Vector2(20f, -20f), new Vector2(300f, 28f),
            new Color(0.2f, 0.9f, 0.3f));

        // 경험치바 (체력바 아래)
        Image expFill = CreateBar(canvas.transform, "ExpBar", uiSprite,
            new Vector2(0f, 1f), new Vector2(20f, -56f), new Vector2(300f, 18f),
            new Color(0.3f, 0.7f, 1f));

        // 레벨 텍스트 (경험치바 옆)
        TextMeshProUGUI levelText = CreateText(canvas.transform, "LevelText", font,
            "Lv. 1", 28, new Vector2(0f, 1f), new Vector2(332f, -20f), new Vector2(120f, 40f),
            TextAlignmentOptions.Left);

        // 강화 카드 패널 (중앙)
        UpgradeCardUI[] cards = CreateUpgradePanel(canvas.transform, font, uiSprite, out GameObject cardPanel);

        // ── 6. 컴포넌트 참조 배선 ──
        SetRef(health, "healthBarFill", healthFill);

        SetRef(level, "upgradeManager", upgrade);
        SetRef(level, "expBarFill", expFill);
        SetRef(level, "levelText", levelText);

        SetRefList(upgrade, "allUpgrades", LoadAllUpgradeData());
        SetRef(upgrade, "cardPanel", cardPanel);
        SetRefArray(upgrade, "cards", cards);
        SetRef(upgrade, "stats", stats);
        SetRef(upgrade, "playerHealth", health);

        // ── 7. GameManager 재배선 (필드명이 바뀌어 자동 연결 안 됨) ──
        GameManager gm = gmObj.GetComponent<GameManager>();
        if (timerGo != null)  SetRef(gm, "timerText", timerGo.GetComponent<TextMeshProUGUI>());
        if (gameOver != null) SetRef(gm, "gameOverPanel", gameOver);
        if (finalGo != null)  SetRef(gm, "finalTimeText", finalGo.GetComponent<TextMeshProUGUI>());

        // ── 8. 카메라 추적 ──
        if (mainCam != null)
        {
            CameraFollow follow = EnsureComponent<CameraFollow>(mainCam.gameObject);
            SetRef(follow, "target", player.transform);
        }

        // ── 9. BallSpawner 제거 ──
        GameObject ballSpawner = GameObject.Find("BallSpawner");
        if (ballSpawner != null)
        {
            Debug.Log("[VS_SceneSetup] BallSpawner 제거");
            Object.DestroyImmediate(ballSpawner);
        }

        // ── 10. 강화 패널은 평소 숨김 ──
        if (cardPanel != null) cardPanel.SetActive(false);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();

        Debug.Log("[VS_SceneSetup] 완료");
    }

    // ── UI 빌더 ──

    static Image CreateBar(Transform parent, string name, Sprite sprite,
                           Vector2 anchor, Vector2 anchoredPos, Vector2 size, Color fillColor)
    {
        // 배경
        GameObject bg = new GameObject(name + "_BG", typeof(RectTransform), typeof(Image));
        SetRect(bg.GetComponent<RectTransform>(), parent, anchor, anchoredPos, size);
        Image bgImg = bg.GetComponent<Image>();
        bgImg.sprite = sprite;
        bgImg.type = Image.Type.Sliced;
        bgImg.color = new Color(0f, 0f, 0f, 0.5f);

        // 채움
        GameObject fill = new GameObject(name + "_Fill", typeof(RectTransform), typeof(Image));
        RectTransform frt = fill.GetComponent<RectTransform>();
        frt.SetParent(bg.transform, false);
        frt.anchorMin = Vector2.zero;
        frt.anchorMax = Vector2.one;
        frt.offsetMin = new Vector2(3f, 3f);
        frt.offsetMax = new Vector2(-3f, -3f);
        Image fillImg = fill.GetComponent<Image>();
        fillImg.sprite = sprite;
        fillImg.type = Image.Type.Filled;
        fillImg.fillMethod = Image.FillMethod.Horizontal;
        fillImg.fillOrigin = (int)Image.OriginHorizontal.Left;
        fillImg.fillAmount = 1f;
        fillImg.color = fillColor;

        return fillImg;
    }

    static TextMeshProUGUI CreateText(Transform parent, string name, TMP_FontAsset font,
                                      string text, float fontSize, Vector2 anchor,
                                      Vector2 anchoredPos, Vector2 size, TextAlignmentOptions align)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        SetRect(go.GetComponent<RectTransform>(), parent, anchor, anchoredPos, size);
        TextMeshProUGUI tmp = go.GetComponent<TextMeshProUGUI>();
        if (font != null) tmp.font = font;
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = align;
        tmp.color = Color.white;
        return tmp;
    }

    static UpgradeCardUI[] CreateUpgradePanel(Transform parent, TMP_FontAsset font, Sprite uiSprite,
                                              out GameObject panel)
    {
        // 반투명 배경 패널 (전체 덮음)
        panel = new GameObject("UpgradePanel", typeof(RectTransform), typeof(Image));
        RectTransform prt = panel.GetComponent<RectTransform>();
        prt.SetParent(parent, false);
        prt.anchorMin = Vector2.zero;
        prt.anchorMax = Vector2.one;
        prt.offsetMin = Vector2.zero;
        prt.offsetMax = Vector2.zero;
        Image panelImg = panel.GetComponent<Image>();
        panelImg.color = new Color(0f, 0f, 0f, 0.75f);

        // 안내 문구
        CreateText(panel.transform, "Title", font, "강화를 선택하세요", 40,
            new Vector2(0.5f, 1f), new Vector2(0f, -120f), new Vector2(600f, 60f),
            TextAlignmentOptions.Center);

        UpgradeCardUI[] cards = new UpgradeCardUI[3];
        float cardW = 260f, cardH = 360f, gap = 40f;
        float totalW = cardW * 3 + gap * 2;
        float startX = -totalW / 2f + cardW / 2f;

        for (int i = 0; i < 3; i++)
        {
            float x = startX + i * (cardW + gap);
            cards[i] = CreateCard(panel.transform, $"Card{i}", font, uiSprite,
                new Vector2(x, 0f), new Vector2(cardW, cardH));
        }

        return cards;
    }

    static UpgradeCardUI CreateCard(Transform parent, string name, TMP_FontAsset font, Sprite uiSprite,
                                    Vector2 centerOffset, Vector2 size)
    {
        // 카드 본체 = 버튼
        GameObject card = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button), typeof(UpgradeCardUI));
        RectTransform rt = card.GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = centerOffset;
        rt.sizeDelta = size;

        Image cardImg = card.GetComponent<Image>();
        cardImg.sprite = uiSprite;
        cardImg.type = Image.Type.Sliced;
        cardImg.color = new Color(0.2f, 0.2f, 0.28f);

        // 아이콘
        GameObject icon = new GameObject("Icon", typeof(RectTransform), typeof(Image));
        SetRect(icon.GetComponent<RectTransform>(), card.transform, new Vector2(0.5f, 1f),
            new Vector2(0f, -90f), new Vector2(120f, 120f));
        Image iconImg = icon.GetComponent<Image>();
        iconImg.enabled = false; // 아이콘 에셋 없으면 숨김. Show()가 켠다

        // 이름
        TextMeshProUGUI nameText = CreateText(card.transform, "Name", font, "강화", 30,
            new Vector2(0.5f, 1f), new Vector2(0f, -180f), new Vector2(240f, 44f),
            TextAlignmentOptions.Center);

        // 설명
        TextMeshProUGUI descText = CreateText(card.transform, "Desc", font, "설명", 22,
            new Vector2(0.5f, 0.5f), new Vector2(0f, -30f), new Vector2(220f, 140f),
            TextAlignmentOptions.Top);

        UpgradeCardUI cardUI = card.GetComponent<UpgradeCardUI>();
        SetRef(cardUI, "iconImage", iconImg);
        SetRef(cardUI, "nameText", nameText);
        SetRef(cardUI, "descriptionText", descText);
        SetRef(cardUI, "button", card.GetComponent<Button>());

        return cardUI;
    }

    static void SetRect(RectTransform rt, Transform parent, Vector2 anchor, Vector2 anchoredPos, Vector2 size)
    {
        rt.SetParent(parent, false);
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = anchor;
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;
    }

    // ── 씬 오브젝트 유틸 ──

    static GameObject ResolveSceneObject(long fileId)
    {
        string idString = $"GlobalObjectId_V1-2-{SceneGuid}-{fileId}-0";
        if (GlobalObjectId.TryParse(idString, out GlobalObjectId gid))
        {
            Object obj = GlobalObjectId.GlobalObjectIdentifierToObjectSlow(gid);
            if (obj is GameObject go) return go;
            if (obj is Component comp) return comp.gameObject;
        }
        Debug.LogWarning($"[VS_SceneSetup] fileID {fileId} 오브젝트를 못 찾음. 해당 UI 배선 건너뜀.");
        return null;
    }

    static T FindOrCreate<T>(string name) where T : Component
    {
        T existing = Object.FindFirstObjectByType<T>();
        if (existing != null) return existing;

        GameObject go = GameObject.Find(name);
        if (go == null) go = new GameObject(name);
        return go.GetComponent<T>() ?? go.AddComponent<T>();
    }

    static T EnsureComponent<T>(GameObject go) where T : Component
    {
        return go.GetComponent<T>() ?? go.AddComponent<T>();
    }

    static GameObject LoadPrefab(string name)
    {
        return AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabDir}/{name}.prefab");
    }

    static List<Object> LoadAllMonsterData()
    {
        return LoadAllInFolder(MonsterDataDir, "t:MonsterData");
    }

    static List<Object> LoadAllUpgradeData()
    {
        return LoadAllInFolder(UpgradeDataDir, "t:UpgradeData");
    }

    static List<Object> LoadAllInFolder(string folder, string filter)
    {
        List<Object> result = new List<Object>();
        string[] guids = AssetDatabase.FindAssets(filter, new[] { folder });
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Object obj = AssetDatabase.LoadAssetAtPath<Object>(path);
            if (obj != null) result.Add(obj);
        }
        return result;
    }

    // ── 참조 배선 (private [SerializeField] 대응) ──

    static void SetRef(Object target, string field, Object value)
    {
        if (target == null) return;
        SerializedObject so = new SerializedObject(target);
        SerializedProperty prop = so.FindProperty(field);
        if (prop == null)
        {
            Debug.LogWarning($"[VS_SceneSetup] 필드 못 찾음: {target.GetType().Name}.{field}");
            return;
        }
        prop.objectReferenceValue = value;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    static void SetRefList(Object target, string field, List<Object> values)
    {
        if (target == null) return;
        SerializedObject so = new SerializedObject(target);
        SerializedProperty prop = so.FindProperty(field);
        if (prop == null)
        {
            Debug.LogWarning($"[VS_SceneSetup] 리스트 필드 못 찾음: {target.GetType().Name}.{field}");
            return;
        }
        prop.arraySize = values.Count;
        for (int i = 0; i < values.Count; i++)
            prop.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    static void SetRefArray(Object target, string field, Component[] values)
    {
        List<Object> list = new List<Object>();
        foreach (Component c in values) list.Add(c);
        SetRefList(target, field, list);
    }
}
