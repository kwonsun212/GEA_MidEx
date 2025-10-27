using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunHitscan : MonoBehaviour
{
    [Header("Refs")]
    public Camera cam;
    public Transform muzzle;         // �ѱ� ��ġ

    [Header("Fire")]
    public int damage = 25;
    public float fireRate = 10f;     // �ʴ� �߻�(10 = 0.1s)
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

        // �ӽ� ������/Ŀ�� ���� �׽�Ʈ Ű (���ϸ� ����)
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
            // ������
            if (hit.collider.TryGetComponent<IDamageable>(out var dmg))
            {
                dmg.TakeDamage(damage, hit.point, hit.normal);
            }

            // ��Ʈ ����Ʈ
            if (hitVFX) Instantiate(hitVFX, hit.point, Quaternion.LookRotation(hit.normal));

            // Ʈ���̼�
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
            // ������ ��� Ʈ���̼� ����
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

