using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Health : MonoBehaviour, IDamageable
{
    public int maxHP = 100;
    public GameObject deathVFX;

    int hp;

    void Awake() => hp = maxHP;

    public void TakeDamage(int amount, Vector3 hitPoint, Vector3 hitNormal)
    {
        hp -= amount;
        // 맞았을 때 간단한 피드백
        // 예: 깜빡임, 히트파티클, 히트사운드 등 (원하면 추가해 드림)

        if (hp <= 0)
        {
            if (deathVFX) Instantiate(deathVFX, hitPoint, Quaternion.LookRotation(hitNormal));
            Destroy(gameObject);
        }
    }
}

