using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class EBullet : MonoBehaviour
{
    [Header("Motion")]
    public float speed = 20f;        // m/s
    public float destroyT = 5f;      // 자동 소멸 시간

    [Header("Hit")]
    public LayerMask hitMask = ~0;   // 맞을 레이어(기본 전부)
    public GameObject hitVFX;

    Rigidbody rb;
    bool done;                       // 중복 처리 방지
    Transform shooter;               // 발사자(적)
    Vector3 startPos;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    void OnEnable()
    {
        done = false;
        startPos = transform.position;
    }

    /// <summary>
    /// 발사 시 호출. 발사자와 초기 방향/속도를 주입하고, 발사자와의 충돌을 무시합니다.
    /// </summary>
    public void Init(Transform shooter, Vector3 dir, float muzzleSpeed)
    {
        this.shooter = shooter;

        // 총알 Forward 정렬 + 속도
        transform.rotation = Quaternion.LookRotation(dir);
        rb.velocity = dir.normalized * (muzzleSpeed > 0 ? muzzleSpeed : speed);

        // 발사자와 충돌 무시(자기 몸/자식 콜라이더 모두)
        if (shooter)
        {
            var bulletCol = GetComponent<Collider>();
            var shooterCols = shooter.GetComponentsInChildren<Collider>();
            foreach (var sc in shooterCols)
                if (bulletCol) Physics.IgnoreCollision(bulletCol, sc, true);
        }

        // 자동 소멸 타이머
        CancelInvoke(nameof(SelfDestruct));
        Invoke(nameof(SelfDestruct), destroyT);
    }

    void SelfDestruct()
    {
        if (done) return;
        done = true;
        Destroy(gameObject);
    }

    void OnCollisionEnter(Collision col)
    {
        if (done) return;

        // 레이어 필터
        if ((hitMask.value & (1 << col.gameObject.layer)) == 0)
            return;

        // 히트 지점/노멀
        var cp = col.GetContact(0);
        Vector3 p = cp.point;
        Vector3 n = cp.normal;

        // (선택) 데미지 인터페이스
        if (col.collider.TryGetComponent<IDamageable>(out var dmg))
            dmg.TakeDamage(10, p, n); // 필요 시 변수화

        // 히트 VFX
        if (hitVFX) Instantiate(hitVFX, p, Quaternion.LookRotation(n));

        done = true;
        Destroy(gameObject);
    }
}
