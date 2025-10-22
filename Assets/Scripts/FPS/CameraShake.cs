using UnityEngine;

public class CameraShake : MonoBehaviour
{
    Vector3 originalPos;
    float time, duration, magnitude;

    void Awake() => originalPos = transform.localPosition;

    public void Shake(float dur, float mag)
    {
        duration = dur; magnitude = mag; time = dur;
    }

    void LateUpdate()
    {
        if (time > 0f)
        {
            time -= Time.deltaTime;
            transform.localPosition = originalPos + Random.insideUnitSphere * magnitude;
            if (time <= 0f) transform.localPosition = originalPos;
        }
    }
}
