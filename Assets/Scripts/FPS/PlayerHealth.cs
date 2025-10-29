using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth: MonoBehaviour
{
    [Header("HP ����")]
    [Min(1)] public float maxHP = 100f;
    private float currentHP;

    [Header("UI")]
    public Slider hpSlider;          // HP �����̴�
    public Image fillImage;          // (����) Fill �̹���
    public Gradient colorByHP;       // (����) HP ���� �׶��̼�
    public Text hpText;              // (����) HP �ؽ�Ʈ (ex: HP: 50 / 100)

    [Header("�ǰ� ����")]
    public float collisionDamage = 5f;   // ���� ����� �� �޴� ������
    public float EBcollisionDamage = 10f;
    public float invincibleTime = 0.5f;   // ���� ���� ���� �ð�
    private bool invincible = false;

    void Awake()
    {
        currentHP = maxHP;
        if (hpSlider == null) hpSlider = FindObjectOfType<Slider>();
        InitUI();
        UpdateUI();
    }

    // --- �浹 ó�� ---
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Enemy"))
            TakeDamage(collisionDamage);
        if (collision.collider.CompareTag("EBullet"))
            TakeDamage(EBcollisionDamage);
    }

    // --- HP ó�� ---
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
        Debug.Log("[PlayerHealth] �÷��̾� ���!");
        // TODO: ��� ó�� (������, ���ӿ��� ��)
    }

    // �׽�Ʈ�� Ű (H=������, J=ȸ��)
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H)) TakeDamage(10f);
        if (Input.GetKeyDown(KeyCode.J)) Heal(10f);
    }
}
