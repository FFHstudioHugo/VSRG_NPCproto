using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class SwarmSpawner : MonoBehaviour
{
    public enum SpawnShape { Box, Sphere }

    [Header("Spawning Area")]
    public SpawnShape spawnShape = SpawnShape.Box;
    public Vector3 boxSize = new Vector3(10, 1, 10);
    public float sphereRadius = 8f;

    [Header("Swarm Settings")]
    public GameObject insectPrefab;
    public int insectCount = 10;
    public SwarmManager swarmManager;

    public bool spawnOnStart = false;

    void Start()
    {
        if (spawnOnStart)
        {
            SpawnInsects();
        }
    }

    public void SpawnInsects()
    {
        if (swarmManager == null || insectPrefab == null)
        {
            Debug.LogError("SwarmManager or InsectPrefab not assigned.");
            return;
        }

        for (int i = 0; i < insectCount; i++)
        {
            Vector3 spawnPos = GetRandomPosition();
            GameObject insect = Instantiate(insectPrefab, spawnPos, Quaternion.identity, transform);
            swarmManager.Register(insect.GetComponent<SwarmInsect>());
        }
    }

    public void RegisterAllChildren()
    {
        if (swarmManager == null)
        {
            Debug.LogError("SwarmManager not assigned.");
            return;
        }

        var insects = GetComponentsInChildren<SwarmInsect>();
        var i = 0;
        foreach (var insect in insects)
        {
            i++;
            swarmManager.Register(insect);
            insect.AssignManager(swarmManager, i);
        }

        Debug.Log($"Registered {insects.Length} insects.");
    }

    Vector3 GetRandomPosition()
    {
        Vector3 offset;
        if (spawnShape == SpawnShape.Box)
        {
            offset = new Vector3(
                Random.Range(-boxSize.x / 2f, boxSize.x / 2f),
                Random.Range(-boxSize.y / 2f, boxSize.y / 2f),
                Random.Range(-boxSize.z / 2f, boxSize.z / 2f)
            );
        }
        else
        {
            offset = Random.insideUnitSphere * sphereRadius;
        }

        return transform.position + offset;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        if (spawnShape == SpawnShape.Box)
            Gizmos.DrawWireCube(transform.position, boxSize);
        else
            Gizmos.DrawWireSphere(transform.position, sphereRadius);
    }
}
