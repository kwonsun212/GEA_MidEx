using UnityEngine;

public class WeaponShotgunProjectile : WeaponBase
{
    [Header("Fire")]
    public KeyCode fireKey = KeyCode.Mouse0;
    public bool semiAuto = true;          // true=Ŭ���� 1��, false=������ ���� ����
    public float fireRate = 1.0f;         // �ʴ� �߻�(���� �����̸� 0.8~1.2 ��õ)
    public int pellets = 8;               // �縴 ��
    public float range = 60f;             // ��ǥ ���� ���ÿ� ���� ����

    [Header("Spread (deg)")]
    public float hipSpread = 8f;          // �㸮��� Ȯ��
    public float adsSpread = 3f;          // ADS Ȯ��
    public float moveSpreadBonus = 2f;    // �̵� �� �߰� Ȯ��

    [Header("Projectile Prefab")]
    public GameObject bulletPrefab;       // Bullet ������ (Rigidbody + Bullet.cs)

    [Header("Projectile Params")]
    public float projectileSpeed = 60f;   // Bullet.Fire()�� ������ �ӵ�
    public int pelletDamage = 12;         // �縴 1�� ������
    public LayerMask hitMask;             // Enemy | Environment (Bullet���� ����)

    [Header("FX")]
    public GameObject muzzleVFX;          // (����) ���� ����Ʈ ������
    public float muzzleVFXLife = 0.08f;   // ���� ����Ʈ ����

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

        // (����) ���� ����Ʈ
        if (muzzleVFX && muzzle)
            Destroy(Instantiate(muzzleVFX, muzzle.position, muzzle.rotation), muzzleVFXLife);

        // Ȯ�� ���� ��� (�� �� ����)
        float spreadDeg = isAiming ? adsSpread : hipSpread;
        bool moving = Mathf.Abs(Input.GetAxisRaw("Horizontal")) + Mathf.Abs(Input.GetAxisRaw("Vertical")) > 0.01f;
        if (moving && !isAiming) spreadDeg += moveSpreadBonus;   // �̵� �߿� �� ������(ADS�� ���� �� ����)
        float spreadRad = spreadDeg * Mathf.Deg2Rad;

        Vector3 camPos = camCached.transform.position;
        Vector3 muzzlePos = muzzle ? muzzle.position : camPos;

        for (int i = 0; i < pellets; i++)
        {
            // 1) ī�޶� ���� ���� �� ����
            Vector2 rnd = Random.insideUnitCircle * spreadRad;
            Vector3 dirFromCam = (camCached.transform.forward
                                  + camCached.transform.right * rnd.x
                                  + camCached.transform.up * rnd.y).normalized;

            // 2) ī�޶󿡼� ����ĳ��Ʈ�� "��ǥ ����" ����
            Vector3 targetPoint;
            if (Physics.Raycast(camPos, dirFromCam, out RaycastHit rh, range, hitMask, QueryTriggerInteraction.Ignore))
                targetPoint = rh.point;
            else
                targetPoint = camPos + dirFromCam * range;

            // 3) �ѱ� �� ��ǥ ���� �������� ���� źȯ �߻� (�з����� ����)
            Vector3 shootDir = (targetPoint - muzzlePos).normalized;

            GameObject go = Instantiate(bulletPrefab);
            var bullet = go.GetComponent<Bullet>();
            if (bullet)
            {
                bullet.damage = pelletDamage;   // �縴 ������ ����
                bullet.hitMask = hitMask;        // ��Ʈ ����ũ ����
                bullet.Fire(muzzlePos, shootDir, projectileSpeed);
            }
            else
            {
                // Ȥ�� Bullet ������Ʈ ���� ��, Rigidbody�����ζ� ����
                var rb = go.GetComponent<Rigidbody>();
                go.transform.SetPositionAndRotation(muzzlePos, Quaternion.LookRotation(shootDir));
                if (rb) rb.velocity = shootDir * projectileSpeed;
            }
        }

        // �ݵ�
        if (look) look.AddRecoil(recoilUp, Random.Range(-recoilSide, recoilSide));

        // �ڵ� ������(�ɼ�)
        if (CurrentAmmo == 0 && ReserveAmmo > 0 && !isReloading)
            StartCoroutine(CoReload());
    }
}
