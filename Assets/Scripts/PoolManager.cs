using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 오브젝트 풀. 몬스터·총알·젬처럼 초당 수십 개가 생겼다 사라지는 것들을
/// 파괴하지 않고 재사용한다.
///
/// 싱글톤인 이유: 총알이 죽는 순간처럼 어디서든 풀에 닿아야 하는 자리가 많은데,
/// 매번 참조를 넘기면 배선이 폭발한다. 이 프로젝트의 싱글톤은 GameManager 와 여기 둘뿐이다.
/// </summary>
public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance { get; private set; }

    [Tooltip("끄면 풀링 없이 Instantiate/Destroy 를 쓴다. 3일차 시연용 — " +
             "끈 채로 후반부를 돌려 렉을 체감시킨 뒤 켠다")]
    [SerializeField] private bool poolingEnabled = true;

    // 프리팹 → 대기 중인(비활성) 인스턴스 큐
    private readonly Dictionary<GameObject, Queue<GameObject>> pools =
        new Dictionary<GameObject, Queue<GameObject>>();

    // 인스턴스 → 어느 프리팹에서 나왔는지. 반납할 때 어느 큐로 보낼지 알아야 한다.
    private readonly Dictionary<GameObject, GameObject> instanceToPrefab =
        new Dictionary<GameObject, GameObject>();

    void Awake()
    {
        Instance = this;
    }

    /// <summary>풀에서 하나 꺼낸다. 없으면 새로 만든다.</summary>
    public GameObject Get(GameObject prefab, Vector3 position)
    {
        if (prefab == null) return null;

        if (!poolingEnabled)
        {
            GameObject fresh = Instantiate(prefab, position, Quaternion.identity);
            instanceToPrefab[fresh] = prefab;
            return fresh;
        }

        if (!pools.TryGetValue(prefab, out Queue<GameObject> queue))
        {
            queue = new Queue<GameObject>();
            pools[prefab] = queue;
        }

        GameObject obj;
        if (queue.Count > 0)
        {
            obj = queue.Dequeue();

            // 풀에 있는 동안 씬 로드 등으로 파괴됐을 수 있다.
            if (obj == null) return Get(prefab, position);

            obj.transform.position = position;
            obj.SetActive(true);
        }
        else
        {
            obj = Instantiate(prefab, position, Quaternion.identity);
            instanceToPrefab[obj] = prefab;
        }

        return obj;
    }

    /// <summary>다 쓴 오브젝트를 풀에 돌려준다. Destroy 대신 이걸 쓴다.</summary>
    public void Release(GameObject obj)
    {
        if (obj == null) return;

        if (!poolingEnabled)
        {
            instanceToPrefab.Remove(obj);
            Destroy(obj);
            return;
        }

        // 이중 반납 방지. 이미 비활성이면 큐에 들어가 있다는 뜻이다.
        // 이걸 막지 않으면 같은 객체가 두 번 대여돼 추적 불가능한 버그가 된다.
        if (!obj.activeSelf) return;

        if (!instanceToPrefab.TryGetValue(obj, out GameObject prefab))
        {
            // 풀이 모르는 객체다. 풀링을 껐다 켠 뒤 남은 것 등.
            Destroy(obj);
            return;
        }

        obj.SetActive(false);
        pools[prefab].Enqueue(obj);
    }

    /// <summary>해당 프리팹의 대기 중인 인스턴스 수. 반납 누락 검증용.</summary>
    public int CountInactive(GameObject prefab)
    {
        return pools.TryGetValue(prefab, out Queue<GameObject> queue) ? queue.Count : 0;
    }
}
