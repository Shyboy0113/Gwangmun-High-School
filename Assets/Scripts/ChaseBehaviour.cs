using UnityEngine;

/// <summary>
/// 가장 단순한 몬스터. 플레이어를 향해 직선으로 쫓아온다.
/// 좌우 반전·이동 애니메이션은 부모(MonsterBehaviour.UpdateVisual)가 공통으로 처리한다.
/// </summary>
public class ChaseBehaviour : MonsterBehaviour
{
    protected override void Tick()
    {
        Chase();
    }

    // ─────────── 학생 빈칸 2 (1일차) ───────────
    // 플레이어 쪽 방향을 구해서 그쪽으로 움직인다.
    // "목표 위치 - 내 위치" 가 목표를 향한 방향 벡터다.
    // 1일차에 PlayerController.Move() 에서 배운 normalized 가 여기서 다시 쓰인다.
    void Chase()
    {
        Vector2 direction = ((Vector2)player.position - (Vector2)transform.position).normalized;
        rb.linearVelocity = direction * data.moveSpeed;
    }
}
