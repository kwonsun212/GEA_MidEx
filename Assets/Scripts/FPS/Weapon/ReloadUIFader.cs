using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class ReloadUIFader : MonoBehaviour
{
    [Header("Fade Settings")]
    public float fadeInDuration = 0.2f;
    public float fadeOutDuration = 0.25f;
    public AnimationCurve fadeInCurve;
    public AnimationCurve fadeOutCurve;

    private CanvasGroup cg;
    private Coroutine fadeCo;

    void Awake()
    {
        cg = GetComponent<CanvasGroup>();
        if (!cg) cg = gameObject.AddComponent<CanvasGroup>();

        // ������Ʈ�� ��� Ȱ��ȭ ���·� �ΰ� ���ĸ� 0����
        cg.alpha = 0f;
        cg.interactable = false;
        cg.blocksRaycasts = false;

    }

    public void ShowReload(bool show)
    {
        // �̹� ���� ������ ����
        if (fadeCo != null) StopCoroutine(fadeCo);

        // ������Ʈ�� �������� ���ɼ� ����(Ȥ�� �ܺο��� ������ �����)
        if (!gameObject.activeSelf) gameObject.SetActive(true);

        fadeCo = StartCoroutine(CoFade(show));
    }

    IEnumerator CoFade(bool show)
    {
        float dur = show ? fadeInDuration : fadeOutDuration;
        float start = cg.alpha;
        float end = show ? 1f : 0f;

        if (dur <= 0f)
        {
            cg.alpha = end;
            yield break;
        }

        float t = 0f;
        while (t < dur)
        {
            t += Time.deltaTime;
            float u = Mathf.Clamp01(t / dur);
            if (show && fadeInCurve != null && fadeInCurve.keys.Length > 0) u = fadeInCurve.Evaluate(u);
            if (!show && fadeOutCurve != null && fadeOutCurve.keys.Length > 0) u = fadeOutCurve.Evaluate(u);
            cg.alpha = Mathf.Lerp(start, end, u);
            yield return null;
        }
        cg.alpha = end;

    }
}
