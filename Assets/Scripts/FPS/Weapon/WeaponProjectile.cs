using UnityEngine;

public class WeaponProjectile : WeaponBase
{
    [Header("Fire")]
    public KeyCode fireKey = KeyCode.Mouse0;
    public float fireRate = 10f;
    public float projectileSpeed = 80f;
    public int damage = 25;
    public LayerMask bulletHitMask;

    [Header("Spread")]
    public float baseSpread = 0.4f;    // deg
    public float adsSpreadMultiplier = 0.4f;
    public float moveSpreadBonus = 0.6f;

    [Header("Refs")]
    public Bullet bulletPrefab;        // ÅºÈ¯ ÇÁ¸®ÆÕ
    public string muzzleKey = "MuzzleFlash"; // Ç® Å° (¼±ÅÃ)

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

        if (!string.IsNullOrEmpty(muzzleKey) && muzzle)
            ObjectPooler.I?.Spawn(muzzleKey, muzzle.position, muzzle.rotation, 0.05f);

        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        float spreadRad = baseSpread * Mathf.Deg2Rad;
        if (isAiming) spreadRad *= adsSpreadMultiplier;
        Vector2 mv = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        if (mv.sqrMagnitude > 0.01f) spreadRad += moveSpreadBonus * Mathf.Deg2Rad;

        Vector2 rnd = Random.insideUnitCircle * spreadRad;
        Vector3 dirFromCam = (cam.transform.forward + cam.transform.right * rnd.x + cam.transform.up * rnd.y).normalized;
        ray = new Ray(cam.transform.position, dirFromCam);

        Vector3 targetPoint;
        if (Physics.Raycast(ray, out RaycastHit hit, 5000f, bulletHitMask, QueryTriggerInteraction.Ignore))
            targetPoint = hit.point;
        else
            targetPoint = cam.transform.position + dirFromCam * 5000f;

        Vector3 spawnPos = muzzle ? muzzle.position : cam.transform.position;
        Vector3 shootDir = (targetPoint - spawnPos).normalized;

        var b = Instantiate(bulletPrefab);
        b.damage = damage;
        b.hitMask = bulletHitMask;
        b.Fire(spawnPos, shootDir, projectileSpeed);
    }
}
