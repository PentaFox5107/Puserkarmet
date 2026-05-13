using UnityEngine;
using UnityEngine.AI;

public class NPCSpawner : MonoBehaviour
{
    [Header("NPC Prefabs")]
    public GameObject[] npcPrefabs;

    [Header("Route Points")]
    public Transform[] routePoints;

    [Header("Spawn Timing")]
    public float minSpawnSeconds = 2f;
    public float maxSpawnSeconds = 5f;

    [Header("NavMesh")]
    public float navMeshSearchRadius = 2f;

    void Start()
    {
        ScheduleNextSpawn(0f);
    }

    void SpawnNPC()
    {
        if (npcPrefabs.Length == 0)
        {
            Debug.LogWarning("No NPC prefabs assigned to NPCSpawner.");
            return;
        }

        if (routePoints.Length < 4)
        {
            Debug.LogWarning("Assign at least 4 route points to NPCSpawner.");
            return;
        }

        int randomNPCIndex = Random.Range(0, npcPrefabs.Length);
        GameObject chosenNPC = npcPrefabs[randomNPCIndex];

        int spawnIndex = Random.Range(0, routePoints.Length);
        int destinationIndex = GetDifferentRandomIndex(spawnIndex);

        Transform spawnPoint = routePoints[spawnIndex];
        Transform destinationPoint = routePoints[destinationIndex];

        if (!NavMesh.SamplePosition(spawnPoint.position, out NavMeshHit spawnHit, navMeshSearchRadius, NavMesh.AllAreas))
        {
            Debug.LogWarning("Spawn point is not close enough to the NavMesh.");
            ScheduleNextSpawn(Random.Range(minSpawnSeconds, maxSpawnSeconds));
            return;
        }

        GameObject spawnedNPC = Instantiate(
            chosenNPC,
            spawnHit.position,
            spawnPoint.rotation
        );

        NPCWalker walker = spawnedNPC.GetComponent<NPCWalker>();

        if (walker != null)
        {
            walker.SetDestination(destinationPoint.position);
        }
        else
        {
            Debug.LogWarning(spawnedNPC.name + " does not have an NPCWalker script.");
        }

        float nextDelay = Random.Range(minSpawnSeconds, maxSpawnSeconds);
        ScheduleNextSpawn(nextDelay);
    }

    int GetDifferentRandomIndex(int excludedIndex)
    {
        int index;

        do
        {
            index = Random.Range(0, routePoints.Length);
        }
        while (index == excludedIndex);

        return index;
    }

    void ScheduleNextSpawn(float delay)
    {
        Invoke(nameof(SpawnNPC), delay);
    }
}