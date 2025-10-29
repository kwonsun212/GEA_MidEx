using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Enemy : MonoBehaviour
{
    [Header("Target")]
    public Transform target;                // �÷��̾� Transform
    public float detectRange = 25f;         // Ž�� �Ÿ�
    public float moveSpeed = 3f;            // �̵� �ӵ�
    public float stopDistance = 10f;        // �ʹ� ��������� ����

    [Header("Attack")]
    public GameObject projectilePrefab;     // �߻�ü ������ (�Ѿ� ��)
    public Transform firePoint;             // �߻� ��ġ
    public float projectileSpeed = 40f;     // �Ѿ� �ӵ�
    public float attackInterval = 1.5f;     // ���� ��Ÿ��(��)
    public float attackRange = 15f;         // �����Ÿ�

    [Header("Rotation")]
    public float turnSpeed = 5f;            // ȸ�� �ӵ�

    private Rigidbody rb;
    private float lastAttackTime = -999f;   // ������ ���� �ð� ���

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true; // ���� ȸ�� ����
    }

    void Start()
    {
        // Ÿ�� �ڵ� �˻�
        if (!target)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player)
                target = player.transform;
        }

        // firePoint�� ������ �ڱ� �ڽ� �������� ���
        if (!firePoint)
            firePoint = transform;
    }

    void FixedUpdate()
    {
        if (!target) return;

        float dist = Vector3.Distance(transform.position, target.position);

        // Ž�� �Ÿ� �ȿ����� �ൿ
        if (dist < detectRange)
        {
            // �÷��̾� ���� ���
            Vector3 dir = (target.position - transform.position).normalized;
            dir.y = 0f;

            // �÷��̾� �ٶ󺸱�
            Quaternion lookRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, turnSpeed * Time.deltaTime);

            // �̵�
            if (dist > stopDistance)
            {
                rb.MovePosition(transform.position + dir * moveSpeed * Time.fixedDeltaTime);
            }

            // �����Ÿ� ���̸� ����
            if (dist <= attackRange && Time.time - lastAttackTime >= attackInterval)
            {
                FireProjectile();
                lastAttackTime = Time.time;
            }
        }
    }

    void FireProjectile()
    {
        if (!projectilePrefab) return;

        // �߻� ���� ���
        Vector3 dir = (target.position + Vector3.up * 1.0f - firePoint.position).normalized;

        // �Ѿ� ������ ����
        GameObject bullet = Instantiate(projectilePrefab, firePoint.position, Quaternion.LookRotation(dir));

        // Rigidbody�� �ӵ� �ο�
        Rigidbody brb = bullet.GetComponent<Rigidbody>();
        if (brb)
            brb.velocity = dir * projectileSpeed;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, stopDistance);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
