using UnityEngine;
using UnityEngine.UI;

public class CrosshairUI : MonoBehaviour
{
    public RectTransform up, down, left, right;
    public float baseGap = 6f;
    public float moveGap = 16f;
    public float lerpSpeed = 12f;

    float targetGap;

    void Update()
    {
        // 단순 이동 입력으로 갭 확대
        bool moving = Mathf.Abs(Input.GetAxisRaw("Horizontal")) + Mathf.Abs(Input.GetAxisRaw("Vertical")) > 0.01f;
        targetGap = Mathf.Lerp(targetGap, moving ? moveGap : baseGap, Time.deltaTime * lerpSpeed);

        if (up) up.anchoredPosition = new Vector2(0, targetGap);
        if (down) down.anchoredPosition = new Vector2(0, -targetGap);
        if (left) left.anchoredPosition = new Vector2(-targetGap, 0);
        if (right) right.anchoredPosition = new Vector2(targetGap, 0);
    }
}
