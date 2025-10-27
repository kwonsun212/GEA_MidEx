using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
    public ReloadUIFader reloadUI;

    [Header("Common")]
    public Camera cam;
    public Transform muzzle;
    public int magazineSize = 30;
    public int reserveAmmo = 90;
    public float reloadTime = 1.9f;
    public bool isReloading;
    public bool isAiming;
    public KeyCode reloadKey = KeyCode.R;

    [Header("ADS")]
    public KeyCode aimKey = KeyCode.Mouse1;
    public float adsFOV = 60f;
    public float hipFOV = 75f;
    public float adsSpeed = 12f; // 전환 부드러움

    protected int currentAmmo;
    protected float nextFireTime;

    protected virtual void Awake()
    {
        if (!cam) cam = Camera.main;
        currentAmmo = magazineSize;
        if (cam) cam.fieldOfView = hipFOV;
    }

    protected virtual void Update()
    {
        HandleADS();
        HandleReloadInput();
    }

    void HandleADS()
    {
        isAiming = Input.GetKey(aimKey);
        if (cam)
        {
            float target = isAiming ? adsFOV : hipFOV;
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, target, Time.deltaTime * adsSpeed);
        }
    }

    void HandleReloadInput()
    {
        if (isReloading) return;
        if (Input.GetKeyDown(reloadKey) && currentAmmo < magazineSize && reserveAmmo > 0)
            StartCoroutine(CoReload());
    }

    public System.Collections.IEnumerator CoReload()
    {
        if (isReloading) yield break;

        isReloading = true;

        //  재장전 UI 페이드 인
        reloadUI?.ShowReload(true);

        yield return new WaitForSeconds(reloadTime);

        int need = magazineSize - currentAmmo;
        int toLoad = Mathf.Min(need, reserveAmmo);
        currentAmmo += toLoad;
        reserveAmmo -= toLoad;

        //  재장전 UI 페이드 아웃
        reloadUI?.ShowReload(false);

        isReloading = false;
        OnAmmoChanged();
    }

    public abstract void TryFire();
    protected void SpendAmmo(int amount = 1)
    {
        currentAmmo = Mathf.Max(0, currentAmmo - amount);
        OnAmmoChanged();
    }

    public int CurrentAmmo => currentAmmo;
    public int ReserveAmmo => reserveAmmo;

    public virtual void OnAmmoChanged() { /* UI 갱신 훅 */ }


}
