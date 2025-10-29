using UnityEngine;

public class WeaponShotgunProjectile : WeaponBase
{
    [Header("Fire")]
    public KeyCode fireKey = KeyCode.Mouse0;
    public bool semiAuto = true;          // true=클릭당 1발, false=누르고 유지 연사
    public float fireRate = 1.0f;         // 초당 발사(펌프 느낌이면 0.8~1.2 추천)
    public int pellets = 8;               // 펠릿 수
    public float range = 60f;             // 목표 지점 샘플용 레이 길이

    [Header("Spread (deg)")]
    public float hipSpread = 8f;          // 허리사격 확산
    public float adsSpread = 3f;          // ADS 확산
    public float moveSpreadBonus = 2f;    // 이동 중 추가 확산

    [Header("Projectile Prefab")]
    public GameObject bulletPrefab;       // Bullet 프리팹 (Rigidbody + Bullet.cs)

    [Header("Projectile Params")]
    public float projectileSpeed = 60f;   // Bullet.Fire()에 전달할 속도
    public int pelletDamage = 12;         // 펠릿 1발 데미지
    public LayerMask hitMask;             // Enemy | Environment (Bullet에도 전달)

    [Header("FX")]
    public GameObject muzzleVFX;          // (선택) 머즐 이펙트 프리팹
    public float muzzleVFXLife = 0.08f;   // 머즐 이펙트 수명

    [Header("Recoil")]
    public float recoilUp = 2.0f;
    public float recoilSide = 0.6f;

    Camera camCached;
    MouseLook look;

    protected override void Awake()
    {
        base.Awake();
        camCached = cam ? cam : Camera.main;
        look = camCached ? camCached.GetComponent<MouseLook>() : null;
    }

    protected override void Update()
    {
        base.Update();

        bool trigger = semiAuto ? Input.GetKeyDown(fireKey) : Input.GetKey(fireKey);
        if (trigger && Time.time >= nextFireTime && !isReloading)
            TryFire();
    }

    public override void TryFire()
    {
        if (CurrentAmmo <= 0)
        {
            StartCoroutine(CoReload());
            return;
        }

        nextFireTime = Time.time + 1f / fireRate;
        SpendAmmo(1);

        // (선택) 머즐 이펙트
        if (muzzleVFX && muzzle)
            Destroy(Instantiate(muzzleVFX, muzzle.position, muzzle.rotation), muzzleVFXLife);

        // 확산 각도 계산 (도 → 라디안)
        float spreadDeg = isAiming ? adsSpread : hipSpread;
        bool moving = Mathf.Abs(Input.GetAxisRaw("Horizontal")) + Mathf.Abs(Input.GetAxisRaw("Vertical")) > 0.01f;
        if (moving && !isAiming) spreadDeg += moveSpreadBonus;   // 이동 중엔 더 퍼지게(ADS일 때는 덜 영향)
        float spreadRad = spreadDeg * Mathf.Deg2Rad;

        Vector3 camPos = camCached.transform.position;
        Vector3 muzzlePos = muzzle ? muzzle.position : camPos;

        for (int i = 0; i < pellets; i++)
        {
            // 1) 카메라 기준 랜덤 콘 방향
            Vector2 rnd = Random.insideUnitCircle * spreadRad;
            Vector3 dirFromCam = (camCached.transform.forward
                                  + camCached.transform.right * rnd.x
                                  + camCached.transform.up * rnd.y).normalized;

            // 2) 카메라에서 레이캐스트로 "목표 지점" 샘플
            Vector3 targetPoint;
            if (Physics.Raycast(camPos, dirFromCam, out RaycastHit rh, range, hitMask, QueryTriggerInteraction.Ignore))
                targetPoint = rh.point;
            else
                targetPoint = camPos + dirFromCam * range;

            // 3) 총구 → 목표 지점 방향으로 실제 탄환 발사 (패럴랙스 보정)
            Vector3 shootDir = (targetPoint - muzzlePos).normalized;

            GameObject go = Instantiate(bulletPrefab);
            var bullet = go.GetComponent<Bullet>();
            if (bullet)
            {
                bullet.damage = pelletDamage;   // 펠릿 데미지 주입
                bullet.hitMask = hitMask;        // 히트 마스크 주입
                bullet.Fire(muzzlePos, shootDir, projectileSpeed);
            }
            else
            {
                // 혹시 Bullet 컴포넌트 누락 시, Rigidbody만으로라도 날림
                var rb = go.GetComponent<Rigidbody>();
                go.transform.SetPositionAndRotation(muzzlePos, Quaternion.LookRotation(shootDir));
                if (rb) rb.velocity = shootDir * projectileSpeed;
            }
        }

        // 반동
        if (look) look.AddRecoil(recoilUp, Random.Range(-recoilSide, recoilSide));

        // 자동 재장전(옵션)
        if (CurrentAmmo == 0 && ReserveAmmo > 0 && !isReloading)
            StartCoroutine(CoReload());
    }
}
