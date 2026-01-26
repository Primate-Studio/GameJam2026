using UnityEngine;
using UnityEngine.AI;
using System;
using UnityEditor.PackageManager;

[RequireComponent(typeof(NavMeshAgent))]
public class ClientMovement : MonoBehaviour
{
    private NavMeshAgent agent;
    public Action OnArrival; // Event que avisarà al Manager quan el client arribi
    private bool isLeaving = false;
    private int slotIndex = -1;
    public ClientAnimationController ClientAnimationController;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        ClientAnimationController = GetComponent<ClientAnimationController>();

    }
    public void MoveTo(Vector3 destination, bool isLeaving, int SlotIndex = -1)
    {
        this.isLeaving = isLeaving;
        this.slotIndex = SlotIndex;
        if (agent == null) return;

        agent.SetDestination(destination);
        StopAllCoroutines();
        StartCoroutine(CheckArrivalRoutine());
    }
    public void SetMovementEnabled(bool enabled)
    {
        if (agent == null) return;

        // Aturem o reprenem l'agent
        agent.isStopped = !enabled;

        if (!enabled)
        {
            // Forcem que la velocitat sigui zero perquè no llisqui per inèrcia
            agent.velocity = Vector3.zero;
        }
    }
    private void Update()
    {
        //Always Look at the camera, except when leaving
        if (isLeaving) return;
        Vector3 lookPos = Camera.main.transform.position - transform.position;
        lookPos.y = 0;  
        Quaternion rotation = Quaternion.LookRotation(lookPos);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * 5f);
    }

    private System.Collections.IEnumerator CheckArrivalRoutine()
    {
        // Esperem un frame perquè el NavMesh calculi el camí
        yield return null; 

        while (agent.pathPending || agent.remainingDistance > agent.stoppingDistance) yield return null;

        if (!isLeaving)
        {
            // Ara cridem al nou mètode de registre
            ClientAnimationController.SetIdle(true);
            OrderSystem.Instance.RegisterClientArrival(this.gameObject, slotIndex);
        }
        else Despawn();
    }

    public void Despawn()
    {
        Destroy(gameObject);
    }
}