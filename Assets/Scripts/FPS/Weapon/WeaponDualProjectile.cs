using UnityEngine;

public class WeaponDualProjectile : WeaponBase
{
    [Header("Fire")]
    public KeyCode fireKey = KeyCode.Mouse0;
    public bool fullAuto = false;           // ���ڵ��̸� false, ����� true
    public float fireRate = 10f;            // �ʴ� ��ü �߼�(��/�� ��)
    public float projectileSpeed = 70f;
    public int damage = 20;
    public LayerMask bulletHitMask;

    [Header("Spread")]
    public float baseSpread = 0.6f;         // deg
    public float adsSpreadMultiplier = 0.5f;
    public float moveSpreadBonus = 0.6f;

    [Header("Dual Pistols")]
    public Transform leftMuzzle;            // �� �ѱ� (�θ� 'muzzle' ������ ���⼱ ���� ����)
    public Transform rightMuzzle;           // �� �ѱ�
    public float handAlternationDelay = 0.08f; // ��/�� ���� ����(�ð� ����)

    [Header("FX / Recoil")]
    public string muzzleKey = "MuzzleFlash";
    public float recoilUp = 0.5f;
    public float recoilSide = 0.35f;       // ��/�� ���⺰�� ��ȣ �ٲ�

    [Header("Projectile")]
    public Bullet bulletPrefab;

    bool nextHandRight = true;             // ó���� �����պ���
    float handReadyTime;                   // ���� �� �߻簡�� �ð�

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

        // ��ü �߻� �ֱ�(���� ���� fireRate)
        nextFireTime = Time.time + 1f / fireRate;

        // � ������ ���� ���� (�� ���� ������ ���)
        if (Time.time < handReadyTime)
            return; // �� ���� ������ �߿��� ��ŵ

        bool fireRight = nextHandRight;
        nextHandRight = !nextHandRight;
        handReadyTime = Time.time + handAlternationDelay;

        // ź 1�� �Һ�
        SpendAmmo(1);

        // ����(ī�޶� �߽�) �� ��ǥ ����
        Vector3 dirFromCam = GetSpreadedAimDir();
        Vector3 targetPoint = GetAimTargetPoint(dirFromCam);

        // �߻� ��ġ/����(���õ� �ѱ� �� ��ǥ����)
        Transform muzzleT = fireRight ? rightMuzzle : leftMuzzle;
        Vector3 spawnPos = (muzzleT ? muzzleT.position : cam.transform.position);
        Vector3 shootDir = (targetPoint - spawnPos).normalized;

        // źȯ
        var b = Instantiate(bulletPrefab);      // ������ ������Ʈ Ǯ ����
        b.damage = damage;
        b.hitMask = bulletHitMask;
        b.Fire(spawnPos, shootDir, projectileSpeed);

        // ���� �÷���
        if (!string.IsNullOrEmpty(muzzleKey) && muzzleT)
            ObjectPooler.I?.Spawn(muzzleKey, muzzleT.position, muzzleT.rotation, 0.05f);

        // �ݵ� (��/�쿡 ���� ������ ��¦)
        var look = cam.GetComponent<MouseLook>();
        if (look)
        {
            float side = fireRight ? +recoilSide : -recoilSide;
            look.AddRecoil(recoilUp, side);
        }

        // źâ�� �ٴڳ��� �ڵ� ������(�ɼ�)
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
