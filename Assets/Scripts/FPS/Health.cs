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
        // �¾��� �� ������ �ǵ��
        // ��: ������, ��Ʈ��ƼŬ, ��Ʈ���� �� (���ϸ� �߰��� �帲)

        if (hp <= 0)
        {
            if (deathVFX) Instantiate(deathVFX, hitPoint, Quaternion.LookRotation(hitNormal));
            Destroy(gameObject);
        }
    }
}

