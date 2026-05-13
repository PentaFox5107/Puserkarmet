using UnityEngine;
using UnityEngine.AI;

public class NPCWalker : MonoBehaviour
{
    public float stoppingDistance = 0.4f;
    public float navMeshSearchRadius = 2f;

    private NavMeshAgent agent;
    private bool hasDestination;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        if (agent == null)
        {
            Debug.LogError(name + " needs a NavMeshAgent component.");
        }
    }

    public void SetDestination(Vector3 destination)
    {
        if (agent == null)
            return;

        if (NavMesh.SamplePosition(destination, out NavMeshHit hit, navMeshSearchRadius, NavMesh.AllAreas))
        {
            agent.stoppingDistance = stoppingDistance;
            agent.SetDestination(hit.position);
            hasDestination = true;
        }
        else
        {
            Debug.LogWarning(name + " could not find a valid NavMesh destination.");
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if (!hasDestination || agent == null)
            return;

        if (agent.pathPending)
            return;

        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            Destroy(gameObject);
        }
    }
}