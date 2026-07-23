using UnityEngine;

/// <summary>
/// 폭발 이펙트. 잠깐 보여주고 저절로 사라진다.
/// 데미지는 Enemy(폭발형)가 처리하고, 여기는 그림만 보여준다.
/// </summary>
public class Explosion : MonoBehaviour
{
    public float lifetime = 0.5f;   // 화면에 보이는 시간(초)

    void Start()
    {
        Destroy(gameObject, lifetime);
    }
}
