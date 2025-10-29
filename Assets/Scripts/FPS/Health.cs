using UnityEngine;
using UnityEngine.UI; 

public class Health : MonoBehaviour, IDamageable
{
    [Header("HP Settings")]
    public int maxHP = 100;

    [Header("UI")]
    public Slider hpSlider; 

    [Header("Death Effect")]
    public GameObject deathVFX;

    private int hp;

    void Awake()
    {
        hp = maxHP;

        // 슬라이더 초기화
        if (hpSlider)
        {
            hpSlider.maxValue = maxHP;
            hpSlider.value = hp;
        }
    }

    public void TakeDamage(int amount, Vector3 hitPoint, Vector3 hitNormal)
    {
        hp -= amount;
        hp = Mathf.Clamp(hp, 0, maxHP);

        // HP UI 업데이트
        if (hpSlider)
            hpSlider.value = hp;

        if (hp <= 0)
        {
            Die(hitPoint);
        }
    }

    void Die(Vector3 hitPoint)
    {
        if (deathVFX)
        {
            Quaternion rot = Quaternion.LookRotation(transform.forward);
            Vector3 spawnPos = hitPoint + transform.up * 0.2f;
            Instantiate(deathVFX, spawnPos, rot);
        }

        Destroy(gameObject);
    }
}
