using UnityEngine;

public class WeaponHitscan : WeaponBase
{
    [Header("Fire")]
    public KeyCode fireKey = KeyCode.Mouse0;
    public float fireRate = 10f;
    public int damage = 25;
    public float range = 1500f;                 // 히트스캔 유효 거리
    public LayerMask bulletHitMask;             // 맞출 레이어(Enemy | Environment)

    [Header("Spread")]
    public float baseSpread = 0.4f;             // deg
    public float adsSpreadMultiplier = 0.4f;    // ADS 시 퍼짐 배수
    public float moveSpreadBonus = 0.6f;        // 이동 시 추가 확산(도)

    [Header("FX")]
    public string muzzleKey = "MuzzleFlash";    // 머즐 플래시 풀 키(선택)
    public LineRenderer tracerPrefab;           // 선택: 트레이서 라인
    public string hitVFXKey = "HitVFX";         // 선택: 피격 VFX 풀 키

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

        // 머즐 플래시
        if (!string.IsNullOrEmpty(muzzleKey) && muzzle)
            ObjectPooler.I?.Spawn(muzzleKey, muzzle.position, muzzle.rotation, 0.05f);

        // 1) 카메라 기준 발사 방향(퍼짐 포함)
        float spreadRad = baseSpread * Mathf.Deg2Rad;
        if (isAiming) spreadRad *= adsSpreadMultiplier;

        Vector2 mv = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        if (mv.sqrMagnitude > 0.01f) spreadRad += moveSpreadBonus * Mathf.Deg2Rad;

        Vector2 rnd = Random.insideUnitCircle * spreadRad;
        Vector3 dirFromCam = (cam.transform.forward
                             + cam.transform.right * rnd.x
                             + cam.transform.up * rnd.y).normalized;

        // 2) 레이캐스트(카메라 중앙 → 목표 지점)
        Vector3 startCam = cam.transform.position;
        Vector3 startMuzzle = muzzle ? muzzle.position : startCam;

        bool hitSomething = Physics.Raycast(startCam, dirFromCam, out RaycastHit hit, range, bulletHitMask, QueryTriggerInteraction.Ignore);
        Vector3 endPoint = hitSomething ? hit.point : (startCam + dirFromCam * range);

        // 3) 데미지 처리
        if (hitSomething)
        {
            if (hit.collider.TryGetComponent<IDamageable>(out var dmg))
                dmg.TakeDamage(damage, hit.point, hit.normal);

            // (선택) 피격 VFX
            if (!string.IsNullOrEmpty(hitVFXKey))
                ObjectPooler.I?.Spawn(hitVFXKey, hit.point, Quaternion.LookRotation(hit.normal), 0.2f);
        }

        // 4) 트레이서(총구 → 히트 지점) 시각화 (선택)
        if (tracerPrefab)
        {
            var tracer = Instantiate(tracerPrefab);
            tracer.SetPosition(0, startMuzzle);
            tracer.SetPosition(1, endPoint);
            Destroy(tracer.gameObject, 0.06f);
        }

        // 5) 탄이 바닥나면 자동 재장전(선택)
        if (CurrentAmmo == 0 && ReserveAmmo > 0 && !isReloading)
            StartCoroutine(CoReload());
    }
}
