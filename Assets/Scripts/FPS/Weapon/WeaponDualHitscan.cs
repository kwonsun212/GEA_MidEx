using UnityEngine;

public class WeaponDualHitscan : WeaponBase
{
    [Header("Fire")]
    public KeyCode fireKey = KeyCode.Mouse0;
    public bool fullAuto = false;                 // 반자동:false, 연사:true
    public float fireRate = 10f;                  // 좌/우 합산 발사 레이트(초당 발수)
    public int damage = 20;
    public float range = 1500f;
    public LayerMask bulletHitMask;

    [Header("Spread")]
    public float baseSpread = 0.6f;               // deg
    public float adsSpreadMultiplier = 0.5f;      // ADS 시 축소
    public float moveSpreadBonus = 0.6f;          // 이동 시 가산

    [Header("Dual Pistols")]
    public Transform leftMuzzle;                  // 좌 총구
    public Transform rightMuzzle;                 // 우 총구
    public float handAlternationDelay = 0.08f;    // 좌/우 교차 간 최소 간격

    [Header("FX / Tracer")]
    public string muzzleKey = "MuzzleFlash";      // 풀 키(선택)
    public LineRenderer tracerPrefab;             // 선택: 트레이서
    public string hitVFXKey = "HitVFX";           // 선택: 피격 VFX 풀 키

    [Header("Recoil")]
    public float recoilUp = 0.5f;
    public float recoilSide = 0.35f;              // 좌/우에 따라 부호 바뀜

    bool nextHandRight = true;                    // 첫 발은 오른손
    float handReadyTime;                          // 다음 손 발사 가능 시각

    protected override void Update()
    {
        base.Update();

        bool trigger = fullAuto ? Input.GetKey(fireKey) : Input.GetKeyDown(fireKey);
        if (trigger && Time.time >= nextFireTime && !isReloading)
            TryFire();
    }

    public override void TryFire()
    {
        if (CurrentAmmo <= 0) { StartCoroutine(CoReload()); return; }

        // 손 교차 딜레이가 남아있으면 스킵
        if (Time.time < handReadyTime) return;

        // 전체 발사 주기(좌/우 합산 레이트)
        nextFireTime = Time.time + 1f / fireRate;

        // 어떤 손으로 쏠지
        bool fireRight = nextHandRight;
        nextHandRight = !nextHandRight;
        handReadyTime = Time.time + handAlternationDelay;

        SpendAmmo(1);

        // 1) 카메라 기준 조준 방향(퍼짐 포함)
        Vector3 dirFromCam = GetSpreadedAimDir();

        // 2) 카메라 레이 → 목표 지점 계산
        Vector3 startCam = cam.transform.position;
        bool hitSomething = Physics.Raycast(startCam, dirFromCam, out RaycastHit hit, range, bulletHitMask, QueryTriggerInteraction.Ignore);
        Vector3 endPoint = hitSomething ? hit.point : (startCam + dirFromCam * range);

        // 3) 총구(선택된 손)에서 트레이서/머즐 및 연출
        Transform muzzleT = fireRight ? rightMuzzle : leftMuzzle;
        Vector3 startMuzzle = muzzleT ? muzzleT.position : startCam;

        // 머즐 플래시
        if (!string.IsNullOrEmpty(muzzleKey) && muzzleT)
            ObjectPooler.I?.Spawn(muzzleKey, muzzleT.position, muzzleT.rotation, 0.05f);

        // 트레이서(선택)
        if (tracerPrefab)
        {
            var tracer = Object.Instantiate(tracerPrefab);
            tracer.SetPosition(0, startMuzzle);
            tracer.SetPosition(1, endPoint);
            Object.Destroy(tracer.gameObject, 0.06f);
        }

        // 4) 데미지 처리 & 히트 VFX
        if (hitSomething)
        {
            if (hit.collider.TryGetComponent<IDamageable>(out var dmg))
                dmg.TakeDamage(damage, hit.point, hit.normal);

            if (!string.IsNullOrEmpty(hitVFXKey))
                ObjectPooler.I?.Spawn(hitVFXKey, hit.point, Quaternion.LookRotation(hit.normal), 0.2f);
        }

        // 5) 반동 (좌/우에 따라 수평 부호 변경)
        var look = cam.GetComponent<MouseLook>();
        if (look)
        {
            float side = fireRight ? +recoilSide : -recoilSide;
            look.AddRecoil(recoilUp, side);
        }

        // 6) 자동 재장전(옵션)
        if (CurrentAmmo == 0 && ReserveAmmo > 0 && !isReloading)
            StartCoroutine(CoReload());
    }

    Vector3 GetSpreadedAimDir()
    {
        float spreadRad = baseSpread * Mathf.Deg2Rad;
        if (isAiming) spreadRad *= adsSpreadMultiplier;

        Vector2 mv = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        if (mv.sqrMagnitude > 0.01f) spreadRad += moveSpreadBonus * Mathf.Deg2Rad;

        Vector2 rnd = Random.insideUnitCircle * spreadRad;
        return (cam.transform.forward + cam.transform.right * rnd.x + cam.transform.up * rnd.y).normalized;
    }
}
