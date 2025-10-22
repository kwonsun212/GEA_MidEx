using UnityEditor.Rendering.LookDev;
using UnityEngine;

public class WeaponHitscan : WeaponBase
{
    [Header("Fire")]
    public KeyCode fireKey = KeyCode.Mouse0;
    public float fireRate = 10f; // 초당 발사
    public int damage = 25;
    public float range = 120f;
    public LayerMask hitMask;
    public float baseSpread = 0.4f; // 도(deg) 개념으로 사용
    public float adsSpreadMultiplier = 0.4f; // ADS 시 퍼짐 감소
    public float moveSpreadBonus = 0.6f; // 이동 중 가산

    [Header("Recoil")]
    public float recoilUp = 0.6f;
    public float recoilSide = 0.2f;

    [Header("FX Keys (ObjectPooler keys)")]
    public string muzzleKey = "MuzzleFlash";
    public string tracerKey = "Tracer";
    public string hitKey = "HitVFX";

    MouseLook look;
    CameraShake shaker;
    HitmarkerUI hitmarker;

    protected override void Awake()
    {
        base.Awake();
        look = cam ? cam.GetComponent<MouseLook>() : null;
        shaker = cam ? cam.GetComponent<CameraShake>() : null;
        hitmarker = FindObjectOfType<HitmarkerUI>();
    }

    protected override void Update()
    {
        base.Update();
        if (Input.GetKey(fireKey) && Time.time >= nextFireTime && !isReloading)
            TryFire();
    }

    public override void TryFire()
    {
        if (CurrentAmmo <= 0) { StartCoroutine(CoReload()); return; }

        nextFireTime = Time.time + 1f / fireRate;
        SpendAmmo(1);

        // Muzzle FX
        if (!string.IsNullOrEmpty(muzzleKey) && muzzle)
            ObjectPooler.I?.Spawn(muzzleKey, muzzle.position, muzzle.rotation, 0.05f);

        // 조준 방향 + 퍼짐
        Vector3 dir = cam.transform.forward;
        float spread = baseSpread * Mathf.Deg2Rad;
        if (isAiming) spread *= adsSpreadMultiplier;
        // 이동 시 가산
        Vector3 planarVel = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        if (planarVel.sqrMagnitude > 0.01f) spread += moveSpreadBonus * Mathf.Deg2Rad;

        // 랜덤 원뿔 분산
        Vector2 rnd = Random.insideUnitCircle * spread;
        dir = (cam.transform.forward + cam.transform.right * rnd.x + cam.transform.up * rnd.y).normalized;

        // 레이캐스트
        Vector3 start = muzzle ? muzzle.position : cam.transform.position;
        if (Physics.Raycast(cam.transform.position, dir, out RaycastHit hit, range, hitMask, QueryTriggerInteraction.Ignore))
        {
            // 트레이서
            if (!string.IsNullOrEmpty(tracerKey) && muzzle)
            {
                var tracer = ObjectPooler.I?.Spawn(tracerKey, start, Quaternion.identity, 0.05f);
                var lr = tracer ? tracer.GetComponent<LineRenderer>() : null;
                if (lr) { lr.SetPosition(0, start); lr.SetPosition(1, hit.point); }
            }

            // 데미지
            if (hit.collider.TryGetComponent<IDamageable>(out var dmg))
            {
                dmg.TakeDamage(damage, hit.point, hit.normal);
                hitmarker?.Ping(); // 히트마커
            }

            // 히트 VFX
            if (!string.IsNullOrEmpty(hitKey))
                ObjectPooler.I?.Spawn(hitKey, hit.point, Quaternion.LookRotation(hit.normal), 0.3f);
        }
        else
        {
            // 트레이서 미스
            if (!string.IsNullOrEmpty(tracerKey) && muzzle)
            {
                var tracer = ObjectPooler.I?.Spawn(tracerKey, start, Quaternion.identity, 0.05f);
                var lr = tracer ? tracer.GetComponent<LineRenderer>() : null;
                if (lr) { lr.SetPosition(0, start); lr.SetPosition(1, start + dir * range); }
            }
        }

        // 반동 & 화면 흔들림
        look?.AddRecoil(recoilUp, Random.Range(-recoilSide, recoilSide));
        shaker?.Shake(0.05f, 0.2f);

        // 탄창 비면 자동 재장전
        if (CurrentAmmo == 0 && ReserveAmmo > 0 && !isReloading)
            StartCoroutine(CoReload());
    }
}
