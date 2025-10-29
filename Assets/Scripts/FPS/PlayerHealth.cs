using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth: MonoBehaviour
{
    [Header("HP 설정")]
    [Min(1)] public float maxHP = 100f;
    private float currentHP;

    [Header("UI")]
    public Slider hpSlider;          // HP 슬라이더
    public Image fillImage;          // (선택) Fill 이미지
    public Gradient colorByHP;       // (선택) HP 색상 그라데이션
    public Text hpText;              // (선택) HP 텍스트 (ex: HP: 50 / 100)

    [Header("피격 설정")]
    public float collisionDamage = 5f;   // 적과 닿았을 때 받는 데미지
    public float EBcollisionDamage = 10f;
    public float invincibleTime = 0.5f;   // 닿은 직후 무적 시간
    private bool invincible = false;

    void Awake()
    {
        currentHP = maxHP;
        if (hpSlider == null) hpSlider = FindObjectOfType<Slider>();
        InitUI();
        UpdateUI();
    }

    // --- 충돌 처리 ---
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Enemy"))
            TakeDamage(collisionDamage);
        if (collision.collider.CompareTag("EBullet"))
            TakeDamage(EBcollisionDamage);
    }

    // --- HP 처리 ---
    public void TakeDamage(float damage)
    {
        if (invincible || currentHP <= 0f) return;
        currentHP = Mathf.Max(0f, currentHP - damage);
        UpdateUI();
        if (invincibleTime > 0f) StartCoroutine(InvincibleDelay());
        if (currentHP <= 0f) OnDeath();
    }

    public void Heal(float amount)
    {
        if (currentHP <= 0f) return;
        currentHP = Mathf.Min(maxHP, currentHP + amount);
        UpdateUI();
    }

    private System.Collections.IEnumerator InvincibleDelay()
    {
        invincible = true;
        yield return new WaitForSeconds(invincibleTime);
        invincible = false;
    }

    private void InitUI()
    {
        if (hpSlider != null)
        {
            hpSlider.minValue = 0;
            hpSlider.maxValue = maxHP;
            hpSlider.value = currentHP;
            hpSlider.interactable = false;
        }
    }

    private void UpdateUI()
    {
        if (hpSlider != null)
        {
            hpSlider.value = currentHP;
            if (fillImage != null && colorByHP.colorKeys.Length > 0)
            {
                float t = Mathf.InverseLerp(0, maxHP, currentHP);
                fillImage.color = colorByHP.Evaluate(t);
            }
        }

        if (hpText != null)
            hpText.text = $"HP: {Mathf.CeilToInt(currentHP)} / {Mathf.CeilToInt(maxHP)}";
    }

    private void OnDeath()
    {
        Debug.Log("[PlayerHealth] 플레이어 사망!");
        // TODO: 사망 처리 (리스폰, 게임오버 등)
    }

    // 테스트용 키 (H=데미지, J=회복)
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H)) TakeDamage(10f);
        if (Input.GetKeyDown(KeyCode.J)) Heal(10f);
    }
}
