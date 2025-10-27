using UnityEngine;

public class WeaponSniper : WeaponBase
{
    [Header("Fire")]
    public KeyCode fireKey = KeyCode.Mouse0;
    public float fireRate = 0.6f;         // 초당 발사(저격: 낮게)
    public int damage = 120;
    public float range = 2000f;
    public LayerMask hitMask;

    [Header("Spread (Hip vs ADS)")]
    public float hipSpread = 5.0f;        // 허리사격: 매우 큼(“엄청 튀는” 느낌)
    public float adsSpread = 0.05f;       // 줌 시: 극소
    public float moveSpreadBonus = 0.8f;  // 이동 중 추가

    [Header("Zoom / Sensitivity")]
    public float sniperAdsFOV = 30f;      // 강한 줌
    public float hipMouseSensMult = 1.0f; // 평상시
    public float adsMouseSensMult = 0.45f;// 줌 시 감도 감소

    [Header("FX")]
    public string muzzleKey = "MuzzleFlash";
    public LineRenderer tracerPrefab;     // 선택

    [Header("UI")]
    public VignetteUIFader vignette;      // Canvas의 비네트 오버레이

    float localNextFire;                  // 추가 타이머(부모 nextFireTime 사용도 가능)
    MouseLook look; float baseSensitivity;

    protected override void Awake()
    {
        base.Awake();
        look = cam ? cam.GetComponent<MouseLook>() : null;
        if (look) baseSensitivity = look.sensitivity;

        // 스나이퍼 전용 FOV는 WeaponBase의 ADS FOV도 덮어씀
        adsFOV = sniperAdsFOV;
    }

    protected override void Update()
    {
        // ADS 처리(부모에서 FOV Lerp/ isAiming 계산)
        base.Update();

        // 줌 시 비네트/감도 연동
        if (look)
            look.sensitivity = baseSensitivity * (isAiming ? adsMouseSensMult : hipMouseSensMult);
        if (vignette) vignette.Show(isAiming);

        // 발사 입력
        bool canFire = Time.time >= localNextFire && !isReloading;
        if (Input.GetKeyDown(fireKey) && canFire)
            TryFire();
    }

    public override void TryFire()
    {
        if (CurrentAmmo <= 0) { StartCoroutine(CoReload()); return; }

        localNextFire = Time.time + 1f / fireRate;
        SpendAmmo(1);

        // 조준 방향 계산(카메라 기준 + 스프레드)
        float spreadDeg = isAiming ? adsSpread : hipSpread;
        float spreadRad = spreadDeg * Mathf.Deg2Rad;

        Vector2 mv = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        if (!isAiming && mv.sqrMagnitude > 0.01f) spreadRad += moveSpreadBonus * Mathf.Deg2Rad;

        Vector2 rnd = Random.insideUnitCircle * spreadRad;
        Vector3 fireDir = (cam.transform.forward + cam.transform.right * rnd.x + cam.transform.up * rnd.y).normalized;

        // 레이캐스트(카메라 기준)
        Vector3 startCam = cam.transform.position;
        Vector3 startMuzzle = muzzle ? muzzle.position : startCam;

        bool hitSomething = Physics.Raycast(startCam, fireDir, out RaycastHit hit, range, hitMask, QueryTriggerInteraction.Ignore);
        Vector3 endPoint = hitSomething ? hit.point : (startCam + fireDir * range);

        // 데미지
        if (hitSomething && hit.collider.TryGetComponent<IDamageable>(out var dmg))
            dmg.TakeDamage(damage, hit.point, hit.normal);

        // 머즐 플래시
        if (!string.IsNullOrEmpty(muzzleKey) && muzzle)
            ObjectPooler.I?.Spawn(muzzleKey, muzzle.position, muzzle.rotation, 0.05f);

        // 트레이서(선택): 총구→충돌점으로 그리기(시각/십자선 일치)
        if (tracerPrefab)
        {
            var tracer = Instantiate(tracerPrefab);
            tracer.SetPosition(0, startMuzzle);
            tracer.SetPosition(1, endPoint);
            Destroy(tracer.gameObject, 0.06f);
        }

        // 반동: 줌 전에는 좀 크게 튀게(옵션)
        var ml = look;
        if (ml)
        {
            float up = isAiming ? 0.6f : 1.6f;
            float side = isAiming ? 0.1f : Random.Range(-0.8f, 0.8f);
            ml.AddRecoil(up, side);
        }

        // 자동 재장전
        if (CurrentAmmo == 0 && ReserveAmmo > 0 && !isReloading)
            StartCoroutine(CoReload());
    }
}
