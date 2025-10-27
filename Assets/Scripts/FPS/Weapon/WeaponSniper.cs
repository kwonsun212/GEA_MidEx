using UnityEngine;

public class WeaponSniper : WeaponBase
{
    [Header("Fire")]
    public KeyCode fireKey = KeyCode.Mouse0;
    public float fireRate = 0.6f;         // �ʴ� �߻�(����: ����)
    public int damage = 120;
    public float range = 2000f;
    public LayerMask hitMask;

    [Header("Spread (Hip vs ADS)")]
    public float hipSpread = 5.0f;        // �㸮���: �ſ� ŭ(����û Ƣ�¡� ����)
    public float adsSpread = 0.05f;       // �� ��: �ؼ�
    public float moveSpreadBonus = 0.8f;  // �̵� �� �߰�

    [Header("Zoom / Sensitivity")]
    public float sniperAdsFOV = 30f;      // ���� ��
    public float hipMouseSensMult = 1.0f; // ����
    public float adsMouseSensMult = 0.45f;// �� �� ���� ����

    [Header("FX")]
    public string muzzleKey = "MuzzleFlash";
    public LineRenderer tracerPrefab;     // ����

    [Header("UI")]
    public VignetteUIFader vignette;      // Canvas�� ���Ʈ ��������

    float localNextFire;                  // �߰� Ÿ�̸�(�θ� nextFireTime ��뵵 ����)
    MouseLook look; float baseSensitivity;

    protected override void Awake()
    {
        base.Awake();
        look = cam ? cam.GetComponent<MouseLook>() : null;
        if (look) baseSensitivity = look.sensitivity;

        // �������� ���� FOV�� WeaponBase�� ADS FOV�� ���
        adsFOV = sniperAdsFOV;
    }

    protected override void Update()
    {
        // ADS ó��(�θ𿡼� FOV Lerp/ isAiming ���)
        base.Update();

        // �� �� ���Ʈ/���� ����
        if (look)
            look.sensitivity = baseSensitivity * (isAiming ? adsMouseSensMult : hipMouseSensMult);
        if (vignette) vignette.Show(isAiming);

        // �߻� �Է�
        bool canFire = Time.time >= localNextFire && !isReloading;
        if (Input.GetKeyDown(fireKey) && canFire)
            TryFire();
    }

    public override void TryFire()
    {
        if (CurrentAmmo <= 0) { StartCoroutine(CoReload()); return; }

        localNextFire = Time.time + 1f / fireRate;
        SpendAmmo(1);

        // ���� ���� ���(ī�޶� ���� + ��������)
        float spreadDeg = isAiming ? adsSpread : hipSpread;
        float spreadRad = spreadDeg * Mathf.Deg2Rad;

        Vector2 mv = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        if (!isAiming && mv.sqrMagnitude > 0.01f) spreadRad += moveSpreadBonus * Mathf.Deg2Rad;

        Vector2 rnd = Random.insideUnitCircle * spreadRad;
        Vector3 fireDir = (cam.transform.forward + cam.transform.right * rnd.x + cam.transform.up * rnd.y).normalized;

        // ����ĳ��Ʈ(ī�޶� ����)
        Vector3 startCam = cam.transform.position;
        Vector3 startMuzzle = muzzle ? muzzle.position : startCam;

        bool hitSomething = Physics.Raycast(startCam, fireDir, out RaycastHit hit, range, hitMask, QueryTriggerInteraction.Ignore);
        Vector3 endPoint = hitSomething ? hit.point : (startCam + fireDir * range);

        // ������
        if (hitSomething && hit.collider.TryGetComponent<IDamageable>(out var dmg))
            dmg.TakeDamage(damage, hit.point, hit.normal);

        // ���� �÷���
        if (!string.IsNullOrEmpty(muzzleKey) && muzzle)
            ObjectPooler.I?.Spawn(muzzleKey, muzzle.position, muzzle.rotation, 0.05f);

        // Ʈ���̼�(����): �ѱ����浹������ �׸���(�ð�/���ڼ� ��ġ)
        if (tracerPrefab)
        {
            var tracer = Instantiate(tracerPrefab);
            tracer.SetPosition(0, startMuzzle);
            tracer.SetPosition(1, endPoint);
            Destroy(tracer.gameObject, 0.06f);
        }

        // �ݵ�: �� ������ �� ũ�� Ƣ��(�ɼ�)
        var ml = look;
        if (ml)
        {
            float up = isAiming ? 0.6f : 1.6f;
            float side = isAiming ? 0.1f : Random.Range(-0.8f, 0.8f);
            ml.AddRecoil(up, side);
        }

        // �ڵ� ������
        if (CurrentAmmo == 0 && ReserveAmmo > 0 && !isReloading)
            StartCoroutine(CoReload());
    }
}
