using UnityEngine;
using UnityEngine.UI;

public class HitmarkerUI : MonoBehaviour
{
    public Image image;
    public float showTime = 0.06f;
    float t;

    void Awake()
    {
        if (!image) image = GetComponentInChildren<Image>(true);
        if (image) image.enabled = false;
    }

    public void Ping()
    {
        t = showTime;
        if (image) image.enabled = true;
    }

    void Update()
    {
        if (!image) return;
        if (t > 0) { t -= Time.deltaTime; if (t <= 0) image.enabled = false; }
    }
}
