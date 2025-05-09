using UnityEngine;
using UnityEngine.AI;


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

            // Snap to NavMesh
            if (NavMesh.SamplePosition(spawnPos, out NavMeshHit navHit, 2f, NavMesh.AllAreas))
            {
                spawnPos = navHit.position;

                // Align with surface normal using raycast
                if (Physics.Raycast(spawnPos + Vector3.up * 0.5f, Vector3.down, out RaycastHit hit, 2f))
                {
                    Quaternion rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);

                    // Apply random Y rotation around surface normal
                    Quaternion randomYRotation = Quaternion.AngleAxis(Random.Range(0f, 360f), hit.normal);
                    Quaternion finalRotation = randomYRotation * rotation;

                    GameObject insect = Instantiate(insectPrefab, spawnPos, finalRotation, transform);
                    swarmManager.Register(insect.GetComponent<SwarmInsect>());
                }
                else
                {
                    // fallback: use upright
                    GameObject insect = Instantiate(insectPrefab, spawnPos, Quaternion.identity, transform);
                    swarmManager.Register(insect.GetComponent<SwarmInsect>());
                }
            }
            else
            {
                Debug.LogWarning($"Insect {i} spawn position not on NavMesh.");
            }
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
