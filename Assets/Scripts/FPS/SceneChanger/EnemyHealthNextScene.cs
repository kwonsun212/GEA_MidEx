using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EnemyHealthNextScene : MonoBehaviour, IDamageable
{
    [Header("HP Settings")]
    public int maxHP = 100;

    [Header("UI")]
    public Slider hpSlider;

    [Header("Death Effect")]
    public GameObject deathVFX;
    public string nextSceneName;

    private int hp;
    private bool dead = false;

    void Awake()
    {
        hp = maxHP;
        if (hpSlider)
        {
            hpSlider.maxValue = maxHP;
            hpSlider.value = hp;
        }
    }

    public void TakeDamage(int amount, Vector3 hitPoint, Vector3 hitNormal)
    {
        if (dead) return;

        hp = Mathf.Clamp(hp - amount, 0, maxHP);
        if (hpSlider) hpSlider.value = hp;

        if (hp <= 0) Die(hitPoint);
    }

    void Die(Vector3 hitPoint)
    {
        if (dead) return;
        dead = true;

        // ��� ����Ʈ
        if (deathVFX)
        {
            var rot = Quaternion.LookRotation(transform.forward);
            var spawnPos = hitPoint + transform.up * 0.2f;
            Instantiate(deathVFX, spawnPos, rot);
        }

        // �浹/�̵� �� ��Ȱ��ȭ (���ϴ� ���)
        var colls = GetComponentsInChildren<Collider>();
        foreach (var c in colls) c.enabled = false;
        var rb = GetComponent<Rigidbody>(); if (rb) rb.isKinematic = true;

        // ���� �� �� �ε� (Ÿ�ӽ����� 0���� ����)
        StartCoroutine(LoadNextAfterDelay(0.5f));
    }

    System.Collections.IEnumerator LoadNextAfterDelay(float delay)
    {
        // ������ �Ͻ����� ���¿��� Ȯ���� ��ٸ����� Realtime ���
        float t = 0f;
        while (t < delay)
        {
            t += Time.unscaledDeltaTime;
            yield return null;
        }

        if (!string.IsNullOrEmpty(nextSceneName))
            SceneManager.LoadScene(nextSceneName, LoadSceneMode.Single);
        // Destroy(gameObject); // �� ��ȯ�ϸ� �ʿ� ����
    }
}
