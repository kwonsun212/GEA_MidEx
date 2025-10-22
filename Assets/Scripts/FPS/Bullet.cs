using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class Bullet : MonoBehaviour
{
    public int damage = 25;
    public float speed = 80f;           // m/s ����
    public float lifeTime = 2f;         // �ڵ� �Ҹ�
    public LayerMask hitMask;           // Enemy | Environment
    public GameObject hitVFX;

    Rigidbody rb;
    float life;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        GetComponent<Collider>().isTrigger = false; // �浹�� ����
    }

    public void Fire(Vector3 pos, Vector3 dir, float muzzleSpeed = -1f)
    {
        transform.SetPositionAndRotation(pos, Quaternion.LookRotation(dir));
        if (muzzleSpeed > 0f) speed = muzzleSpeed;
        rb.velocity = dir * speed;
        life = lifeTime;
        gameObject.SetActive(true);
    }

    void Update()
    {
        life -= Time.deltaTime;
        if (life <= 0f) Destroy(gameObject); // Ǯ�� ���� Despawn���� ��ü
    }

    void OnCollisionEnter(Collision col)
    {
        // ���� ����/���
        var contact = col.GetContact(0);
        var point = contact.point;
        var normal = contact.normal;

        // ������ ó��
        if (col.collider.TryGetComponent<IDamageable>(out var dmg))
            dmg.TakeDamage(damage, point, normal);

        // ��Ʈ VFX
        if (hitVFX) Instantiate(hitVFX, point, Quaternion.LookRotation(normal));

        Destroy(gameObject); // Ǯ���̸� Despawn
    }
}
