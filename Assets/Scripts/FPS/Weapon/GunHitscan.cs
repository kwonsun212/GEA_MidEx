using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunHitscan : MonoBehaviour
{
    [Header("Refs")]
    public Camera cam;
    public Transform muzzle;         // 총구 위치

    [Header("Fire")]
    public int damage = 25;
    public float fireRate = 10f;     // 초당 발사(10 = 0.1s)
    public float range = 100f;
    public LayerMask hitMask;        // Enemy,Environment

    [Header("FX")]
    public GameObject muzzleFlashVFX;
    public GameObject hitVFX;
    public LineRenderer tracerPrefab;

    float nextFire;

    void Reset()
    {
        cam = Camera.main;
    }

    void Update()
    {
        if (Input.GetMouseButton(0) && Time.time >= nextFire)
        {
            nextFire = Time.time + 1f / fireRate;
            Fire();
        }

        // 임시 재장전/커서 해제 테스트 키 (원하면 삭제)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    void Fire()
    {
        if (muzzleFlashVFX) Instantiate(muzzleFlashVFX, muzzle.position, muzzle.rotation);

        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        Vector3 start = muzzle ? muzzle.position : cam.transform.position;

        if (Physics.Raycast(ray, out RaycastHit hit, range, hitMask, QueryTriggerInteraction.Ignore))
        {
            // 데미지
            if (hit.collider.TryGetComponent<IDamageable>(out var dmg))
            {
                dmg.TakeDamage(damage, hit.point, hit.normal);
            }

            // 히트 이펙트
            if (hitVFX) Instantiate(hitVFX, hit.point, Quaternion.LookRotation(hit.normal));

            // 트레이서
            if (tracerPrefab)
            {
                var tracer = Instantiate(tracerPrefab);
                tracer.SetPosition(0, start);
                tracer.SetPosition(1, hit.point);
                Destroy(tracer.gameObject, 0.05f);
            }
        }
        else
        {
            // 빗나간 경우 트레이서 끝점
            if (tracerPrefab)
            {
                var tracer = Instantiate(tracerPrefab);
                tracer.SetPosition(0, start);
                tracer.SetPosition(1, start + cam.transform.forward * range);
                Destroy(tracer.gameObject, 0.05f);
            }
        }
    }
}

