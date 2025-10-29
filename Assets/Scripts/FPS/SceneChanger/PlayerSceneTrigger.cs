using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerSceneTrigger : MonoBehaviour
{
    [Header("Scene Settings")]
    [Tooltip("전환할 다음 씬의 이름 (Build Settings에 등록된 이름)")]
    public string nextSceneName;

    [Tooltip("충돌 시 바로 전환하지 않고 약간의 지연을 둘 수 있음 (0이면 즉시)")]
    public float delay = 0f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log($"플레이어가 트리거에 닿음 → {nextSceneName}로 이동");

            if (delay > 0f)
                Invoke(nameof(LoadNextScene), delay);
            else
                LoadNextScene();
        }
    }

    void LoadNextScene()
    {
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogWarning("nextSceneName이 비어있습니다! Inspector에서 설정하세요.");
        }
    }
}
