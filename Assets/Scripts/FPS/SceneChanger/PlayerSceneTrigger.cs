using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerSceneTrigger : MonoBehaviour
{
    [Header("Scene Settings")]
    [Tooltip("��ȯ�� ���� ���� �̸� (Build Settings�� ��ϵ� �̸�)")]
    public string nextSceneName;

    [Tooltip("�浹 �� �ٷ� ��ȯ���� �ʰ� �ణ�� ������ �� �� ���� (0�̸� ���)")]
    public float delay = 0f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log($"�÷��̾ Ʈ���ſ� ���� �� {nextSceneName}�� �̵�");

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
            Debug.LogWarning("nextSceneName�� ����ֽ��ϴ�! Inspector���� �����ϼ���.");
        }
    }
}
