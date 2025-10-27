using UnityEngine;

public class WeaponDualProjectile : WeaponBase
{
    [Header("Fire")]
    public KeyCode fireKey = KeyCode.Mouse0;
    public bool fullAuto = false;           // 반자동이면 false, 연사면 true
    public float fireRate = 10f;            // 초당 전체 발수(좌/우 합)
    public float projectileSpeed = 70f;
    public int damage = 20;
    public LayerMask bulletHitMask;

    [Header("Spread")]
    public float baseSpread = 0.6f;         // deg
    public float adsSpreadMultiplier = 0.5f;
    public float moveSpreadBonus = 0.6f;

    [Header("Dual Pistols")]
    public Transform leftMuzzle;            // 좌 총구 (부모에 'muzzle' 있지만 여기선 쓰지 않음)
    public Transform rightMuzzle;           // 우 총구
    public float handAlternationDelay = 0.08f; // 좌/우 교차 지연(시각 리듬)

    [Header("FX / Recoil")]
    public string muzzleKey = "MuzzleFlash";
    public float recoilUp = 0.5f;
    public float recoilSide = 0.35f;       // 좌/우 방향별로 부호 바뀜

    [Header("Projectile")]
    public Bullet bulletPrefab;

    bool nextHandRight = true;             // 처음엔 오른손부터
    float handReadyTime;                   // 다음 손 발사가능 시각

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

        // 전체 발사 주기(둘이 합쳐 fireRate)
        nextFireTime = Time.time + 1f / fireRate;

        // 어떤 손으로 쏠지 결정 (손 리듬 지연도 고려)
        if (Time.time < handReadyTime)
            return; // 손 교차 딜레이 중에는 스킵

        bool fireRight = nextHandRight;
        nextHandRight = !nextHandRight;
        handReadyTime = Time.time + handAlternationDelay;

        // 탄 1발 소비
        SpendAmmo(1);

        // 조준(카메라 중심) → 목표 지점
        Vector3 dirFromCam = GetSpreadedAimDir();
        Vector3 targetPoint = GetAimTargetPoint(dirFromCam);

        // 발사 위치/방향(선택된 총구 → 목표지점)
        Transform muzzleT = fireRight ? rightMuzzle : leftMuzzle;
        Vector3 spawnPos = (muzzleT ? muzzleT.position : cam.transform.position);
        Vector3 shootDir = (targetPoint - spawnPos).normalized;

        // 탄환
        var b = Instantiate(bulletPrefab);      // 실전은 오브젝트 풀 권장
        b.damage = damage;
        b.hitMask = bulletHitMask;
        b.Fire(spawnPos, shootDir, projectileSpeed);

        // 머즐 플래시
        if (!string.IsNullOrEmpty(muzzleKey) && muzzleT)
            ObjectPooler.I?.Spawn(muzzleKey, muzzleT.position, muzzleT.rotation, 0.05f);

        // 반동 (좌/우에 따라 옆으로 살짝)
        var look = cam.GetComponent<MouseLook>();
        if (look)
        {
            float side = fireRight ? +recoilSide : -recoilSide;
            look.AddRecoil(recoilUp, side);
        }

        // 탄창이 바닥나면 자동 재장전(옵션)
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

    Vector3 GetAimTargetPoint(Vector3 dirFromCam)
    {
        Ray ray = new Ray(cam.transform.position, dirFromCam);
        if (Physics.Raycast(ray, out RaycastHit hit, 5000f, bulletHitMask, QueryTriggerInteraction.Ignore))
            return hit.point;
        return cam.transform.position + dirFromCam * 5000f;
    }
}
