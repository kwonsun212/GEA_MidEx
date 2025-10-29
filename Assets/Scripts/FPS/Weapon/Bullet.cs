using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class Bullet : MonoBehaviour
{
    public int damage = 25;
    public float speed = 80f;           // m/s 느낌
    public float lifeTime = 2f;         // 자동 소멸
    public LayerMask hitMask;           // Enemy | Environment
    public GameObject hitVFX;

    private Rigidbody rb;
    private float life;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        GetComponent<Collider>().isTrigger = false; // 충돌로 판정
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
        if (life <= 0f)
            Destroy(gameObject); // 풀링 쓰면 Despawn으로 교체
    }

    void OnCollisionEnter(Collision col)
    {
        // --- Enemy 태그 감지 ---
        if (col.collider.CompareTag("Enemy"))
        {
            // 적에 맞으면 바로 삭제
            if (hitVFX)
                Instantiate(hitVFX, col.GetContact(0).point, Quaternion.identity);

            Destroy(gameObject);
            return; // 아래 로직은 실행 안 함
        }

        // --- 일반 데미지 처리 ---
        var contact = col.GetContact(0);
        var point = contact.point;
        var normal = contact.normal;

        if (col.collider.TryGetComponent<IDamageable>(out var dmg))
            dmg.TakeDamage(damage, point, normal);

        if (hitVFX)
            Instantiate(hitVFX, point, Quaternion.LookRotation(normal));

        Destroy(gameObject);
    }
}
