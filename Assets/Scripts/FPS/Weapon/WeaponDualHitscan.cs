using UnityEngine;

public class WeaponDualHitscan : WeaponBase
{
    [Header("Fire")]
    public KeyCode fireKey = KeyCode.Mouse0;
    public bool fullAuto = false;                 // ���ڵ�:false, ����:true
    public float fireRate = 10f;                  // ��/�� �ջ� �߻� ����Ʈ(�ʴ� �߼�)
    public int damage = 20;
    public float range = 1500f;
    public LayerMask bulletHitMask;

    [Header("Spread")]
    public float baseSpread = 0.6f;               // deg
    public float adsSpreadMultiplier = 0.5f;      // ADS �� ���
    public float moveSpreadBonus = 0.6f;          // �̵� �� ����

    [Header("Dual Pistols")]
    public Transform leftMuzzle;                  // �� �ѱ�
    public Transform rightMuzzle;                 // �� �ѱ�
    public float handAlternationDelay = 0.08f;    // ��/�� ���� �� �ּ� ����

    [Header("FX / Tracer")]
    public string muzzleKey = "MuzzleFlash";      // Ǯ Ű(����)
    public LineRenderer tracerPrefab;             // ����: Ʈ���̼�
    public string hitVFXKey = "HitVFX";           // ����: �ǰ� VFX Ǯ Ű

    [Header("Recoil")]
    public float recoilUp = 0.5f;
    public float recoilSide = 0.35f;              // ��/�쿡 ���� ��ȣ �ٲ�

    bool nextHandRight = true;                    // ù ���� ������
    float handReadyTime;                          // ���� �� �߻� ���� �ð�

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

        // �� ���� �����̰� ���������� ��ŵ
        if (Time.time < handReadyTime) return;

        // ��ü �߻� �ֱ�(��/�� �ջ� ����Ʈ)
        nextFireTime = Time.time + 1f / fireRate;

        // � ������ ����
        bool fireRight = nextHandRight;
        nextHandRight = !nextHandRight;
        handReadyTime = Time.time + handAlternationDelay;

        SpendAmmo(1);

        // 1) ī�޶� ���� ���� ����(���� ����)
        Vector3 dirFromCam = GetSpreadedAimDir();

        // 2) ī�޶� ���� �� ��ǥ ���� ���
        Vector3 startCam = cam.transform.position;
        bool hitSomething = Physics.Raycast(startCam, dirFromCam, out RaycastHit hit, range, bulletHitMask, QueryTriggerInteraction.Ignore);
        Vector3 endPoint = hitSomething ? hit.point : (startCam + dirFromCam * range);

        // 3) �ѱ�(���õ� ��)���� Ʈ���̼�/���� �� ����
        Transform muzzleT = fireRight ? rightMuzzle : leftMuzzle;
        Vector3 startMuzzle = muzzleT ? muzzleT.position : startCam;

        // ���� �÷���
        if (!string.IsNullOrEmpty(muzzleKey) && muzzleT)
            ObjectPooler.I?.Spawn(muzzleKey, muzzleT.position, muzzleT.rotation, 0.05f);

        // Ʈ���̼�(����)
        if (tracerPrefab)
        {
            var tracer = Object.Instantiate(tracerPrefab);
            tracer.SetPosition(0, startMuzzle);
            tracer.SetPosition(1, endPoint);
            Object.Destroy(tracer.gameObject, 0.06f);
        }

        // 4) ������ ó�� & ��Ʈ VFX
        if (hitSomething)
        {
            if (hit.collider.TryGetComponent<IDamageable>(out var dmg))
                dmg.TakeDamage(damage, hit.point, hit.normal);

            if (!string.IsNullOrEmpty(hitVFXKey))
                ObjectPooler.I?.Spawn(hitVFXKey, hit.point, Quaternion.LookRotation(hit.normal), 0.2f);
        }

        // 5) �ݵ� (��/�쿡 ���� ���� ��ȣ ����)
        var look = cam.GetComponent<MouseLook>();
        if (look)
        {
            float side = fireRight ? +recoilSide : -recoilSide;
            look.AddRecoil(recoilUp, side);
        }

        // 6) �ڵ� ������(�ɼ�)
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
