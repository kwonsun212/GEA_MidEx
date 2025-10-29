using UnityEngine;

public class WeaponHitscan : WeaponBase
{
    [Header("Fire")]
    public KeyCode fireKey = KeyCode.Mouse0;
    public float fireRate = 10f;
    public int damage = 25;
    public float range = 1500f;                 // ��Ʈ��ĵ ��ȿ �Ÿ�
    public LayerMask bulletHitMask;             // ���� ���̾�(Enemy | Environment)

    [Header("Spread")]
    public float baseSpread = 0.4f;             // deg
    public float adsSpreadMultiplier = 0.4f;    // ADS �� ���� ���
    public float moveSpreadBonus = 0.6f;        // �̵� �� �߰� Ȯ��(��)

    [Header("FX")]
    public string muzzleKey = "MuzzleFlash";    // ���� �÷��� Ǯ Ű(����)
    public LineRenderer tracerPrefab;           // ����: Ʈ���̼� ����
    public string hitVFXKey = "HitVFX";         // ����: �ǰ� VFX Ǯ Ű

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

        // ���� �÷���
        if (!string.IsNullOrEmpty(muzzleKey) && muzzle)
            ObjectPooler.I?.Spawn(muzzleKey, muzzle.position, muzzle.rotation, 0.05f);

        // 1) ī�޶� ���� �߻� ����(���� ����)
        float spreadRad = baseSpread * Mathf.Deg2Rad;
        if (isAiming) spreadRad *= adsSpreadMultiplier;

        Vector2 mv = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        if (mv.sqrMagnitude > 0.01f) spreadRad += moveSpreadBonus * Mathf.Deg2Rad;

        Vector2 rnd = Random.insideUnitCircle * spreadRad;
        Vector3 dirFromCam = (cam.transform.forward
                             + cam.transform.right * rnd.x
                             + cam.transform.up * rnd.y).normalized;

        // 2) ����ĳ��Ʈ(ī�޶� �߾� �� ��ǥ ����)
        Vector3 startCam = cam.transform.position;
        Vector3 startMuzzle = muzzle ? muzzle.position : startCam;

        bool hitSomething = Physics.Raycast(startCam, dirFromCam, out RaycastHit hit, range, bulletHitMask, QueryTriggerInteraction.Ignore);
        Vector3 endPoint = hitSomething ? hit.point : (startCam + dirFromCam * range);

        // 3) ������ ó��
        if (hitSomething)
        {
            if (hit.collider.TryGetComponent<IDamageable>(out var dmg))
                dmg.TakeDamage(damage, hit.point, hit.normal);

            // (����) �ǰ� VFX
            if (!string.IsNullOrEmpty(hitVFXKey))
                ObjectPooler.I?.Spawn(hitVFXKey, hit.point, Quaternion.LookRotation(hit.normal), 0.2f);
        }

        // 4) Ʈ���̼�(�ѱ� �� ��Ʈ ����) �ð�ȭ (����)
        if (tracerPrefab)
        {
            var tracer = Instantiate(tracerPrefab);
            tracer.SetPosition(0, startMuzzle);
            tracer.SetPosition(1, endPoint);
            Destroy(tracer.gameObject, 0.06f);
        }

        // 5) ź�� �ٴڳ��� �ڵ� ������(����)
        if (CurrentAmmo == 0 && ReserveAmmo > 0 && !isReloading)
            StartCoroutine(CoReload());
    }
}
