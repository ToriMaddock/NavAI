using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TargetAgent : MonoBehaviour {
    public GameObject hunter;
    private HunterAgent hunterAgent;

    private NavMeshAgent navAgent;

    public float fleeDistance = 1.0f;

    private Vector3 startPosition;
    private Vector3 finalPosition;
    private Vector3 fleePosition;

    void Start()
    {
        navAgent = GetComponent<NavMeshAgent>();
        startPosition = transform.position;
        hunterAgent = hunter.GetComponent<HunterAgent>();
        Wander();
    }

    //if the target has reached its position specified, then recalculate a new position to go to.
    void Update()
    {
        if (Vector3.Distance(transform.position, hunter.transform.position) < fleeDistance
            && (Vector3.Distance(finalPosition, hunter.transform.position) < fleeDistance
            || Vector3.Distance(fleePosition, hunter.transform.position) < fleeDistance))
        {
            Flee();
        }

        if (Vector3.Distance(finalPosition, hunter.transform.position) < fleeDistance
            || Vector3.Distance(fleePosition, hunter.transform.position) < fleeDistance)
        {
            Wander();
        }

        if (!navAgent.pathPending)
        {
            if (navAgent.remainingDistance <= navAgent.stoppingDistance)
            {
                if (!navAgent.hasPath || navAgent.velocity.sqrMagnitude == 0f)
                {
                    Wander();
                }
            }
        }
    }

    /*calculate a random position in the navmesh to go that is a certain distance away 
     * from the hunters. If the target is within this distance already, flee.*/

    void Wander()
    {
        float walkRadius = 8f;
        Vector3 randomDir = Random.insideUnitSphere * walkRadius;
        randomDir += transform.position;
        NavMeshHit hit;
        NavMesh.SamplePosition(randomDir, out hit, walkRadius, 1);
        Vector3 finalPosition = hit.position;

        if (Vector3.Distance(finalPosition, hunter.transform.position) > fleeDistance 
            && Vector3.Distance(transform.position, hunter.transform.position) > fleeDistance)
        {
            navAgent.SetDestination(finalPosition);
        }
    }

    //try to flee a certain distance from the hunters.
    void Flee()
    {
        float distance = Vector3.Distance(transform.position, hunter.transform.position);

        if (distance < fleeDistance)
        {
            Vector3 dirToHunter = transform.position - hunter.transform.position;
            Vector3 fleePosition = transform.position + dirToHunter;
            navAgent.SetDestination(fleePosition);
        }
    }

    public void Reset()
    {
        transform.position = startPosition;
    }
    
}