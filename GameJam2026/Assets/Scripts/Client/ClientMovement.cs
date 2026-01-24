using UnityEngine;
using UnityEngine.AI;
using System;

[RequireComponent(typeof(NavMeshAgent))]
public class ClientMovement : MonoBehaviour
{
    private NavMeshAgent agent;
    public Action OnArrival; // Event que avisarà al Manager quan el client arribi

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    public void MoveTo(Vector3 destination)
    {
        if (agent == null) return;
        agent.SetDestination(destination);
        StopAllCoroutines();
        StartCoroutine(CheckArrivalRoutine());
    }
    private void Update()
    {
        //Always Look at the camera
        Vector3 lookPos = Camera.main.transform.position - transform.position;
        lookPos.y = 0;  
        Quaternion rotation = Quaternion.LookRotation(lookPos);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * 5f);
    }

    private System.Collections.IEnumerator CheckArrivalRoutine()
    {
        // Esperem un frame perquè el NavMesh calculi el camí
        yield return null; 

        while (agent.pathPending || agent.remainingDistance > agent.stoppingDistance)
        {
            yield return null;
        }

        // Si arribem aquí, hem arribat al destí
        OnArrival?.Invoke(); 
    }

    public void Despawn()
    {
        Destroy(gameObject);
    }
}