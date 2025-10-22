using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    public static ObjectPooler I;
    [System.Serializable] public class Pool { public string key; public GameObject prefab; public int size = 8; }
    public List<Pool> pools;
    private Dictionary<string, Queue<GameObject>> dict;

    void Awake()
    {
        if (I && I != this) { Destroy(gameObject); return; }
        I = this; DontDestroyOnLoad(gameObject);
        dict = new Dictionary<string, Queue<GameObject>>();
        foreach (var p in pools)
        {
            var q = new Queue<GameObject>();
            for (int i = 0; i < p.size; i++)
            {
                var go = Instantiate(p.prefab, transform);
                go.SetActive(false);
                q.Enqueue(go);
            }
            dict[p.key] = q;
        }
    }

    public GameObject Spawn(string key, Vector3 pos, Quaternion rot, float autoDespawn = -1f)
    {
        if (!dict.ContainsKey(key)) return null;
        var q = dict[key];
        GameObject go = q.Count > 0 ? q.Dequeue() : Instantiate(pools.Find(pp => pp.key == key).prefab, transform);
        go.transform.SetPositionAndRotation(pos, rot);
        go.SetActive(true);
        if (autoDespawn > 0) StartCoroutine(DespawnAfter(go, key, autoDespawn));
        return go;
    }

    public void Despawn(string key, GameObject go)
    {
        go.SetActive(false);
        dict[key].Enqueue(go);
    }

    System.Collections.IEnumerator DespawnAfter(GameObject go, string key, float t)
    {
        yield return new WaitForSeconds(t);
        if (go) Despawn(key, go);
    }
}
