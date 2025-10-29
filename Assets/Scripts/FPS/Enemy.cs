using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Enemy : MonoBehaviour
{
    [Header("Target")]
    public Transform target;                // 플레이어 Transform
    public float detectRange = 25f;         // 탐지 거리
    public float moveSpeed = 3f;            // 이동 속도
    public float stopDistance = 10f;        // 너무 가까워지면 멈춤

    [Header("Attack")]
    public GameObject projectilePrefab;     // 발사체 프리팹 (총알 등)
    public Transform firePoint;             // 발사 위치
    public float projectileSpeed = 40f;     // 총알 속도
    public float attackInterval = 1.5f;     // 공격 쿨타임(초)
    public float attackRange = 15f;         // 사정거리

    [Header("Rotation")]
    public float turnSpeed = 5f;            // 회전 속도

    private Rigidbody rb;
    private float lastAttackTime = -999f;   // 마지막 공격 시간 기록

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true; // 물리 회전 방지
    }

    void Start()
    {
        // 타겟 자동 검색
        if (!target)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player)
                target = player.transform;
        }

        // firePoint가 없으면 자기 자신 기준으로 쏘기
        if (!firePoint)
            firePoint = transform;
    }

    void FixedUpdate()
    {
        if (!target) return;

        float dist = Vector3.Distance(transform.position, target.position);

        // 탐지 거리 안에서만 행동
        if (dist < detectRange)
        {
            // 플레이어 방향 계산
            Vector3 dir = (target.position - transform.position).normalized;
            dir.y = 0f;

            // 플레이어 바라보기
            Quaternion lookRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, turnSpeed * Time.deltaTime);

            // 이동
            if (dist > stopDistance)
            {
                rb.MovePosition(transform.position + dir * moveSpeed * Time.fixedDeltaTime);
            }

            // 사정거리 안이면 공격
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

        // 발사 방향 계산
        Vector3 dir = (target.position + Vector3.up * 1.0f - firePoint.position).normalized;

        // 총알 프리팹 생성
        GameObject bullet = Instantiate(projectilePrefab, firePoint.position, Quaternion.LookRotation(dir));

        // Rigidbody에 속도 부여
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
