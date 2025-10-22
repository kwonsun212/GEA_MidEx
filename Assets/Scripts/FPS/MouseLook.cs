using UnityEngine;

public class MouseLook : MonoBehaviour
{
    public Transform playerBody;
    public float sensitivity = 200f;
    public float minPitch = -80f;
    public float maxPitch = 80f;

    float pitch;
    float recoilPitch, recoilYaw;
    float recoilReturnSpeed = 10f;

    void Start()
    {
        if (!playerBody) playerBody = transform.parent;
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity * Time.deltaTime;

        // Recoil ȸ��
        recoilPitch = Mathf.Lerp(recoilPitch, 0f, recoilReturnSpeed * Time.deltaTime);
        recoilYaw = Mathf.Lerp(recoilYaw, 0f, recoilReturnSpeed * Time.deltaTime);

        pitch -= mouseY + recoilPitch;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        playerBody.Rotate(Vector3.up * (mouseX + recoilYaw));
    }

    public void AddRecoil(float up, float sideways)
    {
        recoilPitch += up;      // up�� ���� Ʀ(���� �Է� �� �Ʒ���)
        recoilYaw += sideways; // �¿� Ʀ
    }
}
