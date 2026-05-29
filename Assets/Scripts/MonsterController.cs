using System;
using UnityEngine;

public class MonsterController : MonoBehaviour
{
    private GameObject _player;
    private Rigidbody2D _rigidbody2D;

    private int _Hp = 3;

    [SerializeField] private float moveSpeed;
    
    private void Awake()
    {
        _player = GameObject.FindGameObjectWithTag("Player");
        _rigidbody2D = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (_player != null)
        {
            // 플레이어를 향해 이동
            if (_rigidbody2D != null)
            {
                _rigidbody2D.linearVelocityX = moveSpeed * (_player.transform.position.x - transform.position.x);}
        }   
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Bullet"))
        {
            _Hp -= 1;
            if (_Hp <= 0)
                Destroy(gameObject);
        }
    }
}