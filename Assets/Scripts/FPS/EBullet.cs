using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class EBullet : MonoBehaviour
{
    [Header("Motion")]
    public float speed = 20f;        // m/s
    public float destroyT = 5f;      // �ڵ� �Ҹ� �ð�

    [Header("Hit")]
    public LayerMask hitMask = ~0;   // ���� ���̾�(�⺻ ����)
    public GameObject hitVFX;

    Rigidbody rb;
    bool done;                       // �ߺ� ó�� ����
    Transform shooter;               // �߻���(��)
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
    /// �߻� �� ȣ��. �߻��ڿ� �ʱ� ����/�ӵ��� �����ϰ�, �߻��ڿ��� �浹�� �����մϴ�.
    /// </summary>
    public void Init(Transform shooter, Vector3 dir, float muzzleSpeed)
    {
        this.shooter = shooter;

        // �Ѿ� Forward ���� + �ӵ�
        transform.rotation = Quaternion.LookRotation(dir);
        rb.velocity = dir.normalized * (muzzleSpeed > 0 ? muzzleSpeed : speed);

        // �߻��ڿ� �浹 ����(�ڱ� ��/�ڽ� �ݶ��̴� ���)
        if (shooter)
        {
            var bulletCol = GetComponent<Collider>();
            var shooterCols = shooter.GetComponentsInChildren<Collider>();
            foreach (var sc in shooterCols)
                if (bulletCol) Physics.IgnoreCollision(bulletCol, sc, true);
        }

        // �ڵ� �Ҹ� Ÿ�̸�
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

        // ���̾� ����
        if ((hitMask.value & (1 << col.gameObject.layer)) == 0)
            return;

        // ��Ʈ ����/���
        var cp = col.GetContact(0);
        Vector3 p = cp.point;
        Vector3 n = cp.normal;

        // (����) ������ �������̽�
        if (col.collider.TryGetComponent<IDamageable>(out var dmg))
            dmg.TakeDamage(10, p, n); // �ʿ� �� ����ȭ

        // ��Ʈ VFX
        if (hitVFX) Instantiate(hitVFX, p, Quaternion.LookRotation(n));

        done = true;
        Destroy(gameObject);
    }
}
