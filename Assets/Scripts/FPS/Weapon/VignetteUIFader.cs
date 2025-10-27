using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class VignetteUIFader : MonoBehaviour
{
    public float fadeIn = 0.15f;
    public float fadeOut = 0.15f;
    public AnimationCurve inCurve, outCurve;

    CanvasGroup cg; Coroutine co;
    void Awake() { cg = GetComponent<CanvasGroup>(); cg.alpha = 0f; }

    public void Show(bool on)
    {
        if (co != null) StopCoroutine(co);
        co = StartCoroutine(Fade(on));
    }

    IEnumerator Fade(bool on)
    {
        float d = on ? fadeIn : fadeOut, t = 0f, s = cg.alpha, e = on ? 1f : 0f;
        if (d <= 0f) { cg.alpha = e; yield break; }
        while (t < d)
        {
            t += Time.deltaTime;
            float u = Mathf.Clamp01(t / d);
            if (on && inCurve != null && inCurve.keys.Length > 0) u = inCurve.Evaluate(u);
            if (!on && outCurve != null && outCurve.keys.Length > 0) u = outCurve.Evaluate(u);
            cg.alpha = Mathf.Lerp(s, e, u);
            yield return null;
        }
        cg.alpha = e;
    }
}
