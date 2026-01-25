using UnityEngine;
using UnityEngine.AI;

public class ClientAnimationController : MonoBehaviour
{
    private Animator anim;
    private NavMeshAgent agent;

    void Awake()
    {
        anim = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (agent != null)
        {
            float currentSpeed = agent.velocity.magnitude;
            anim.SetFloat("Speed", currentSpeed);
        }
    }

    public void SetTalking(bool state) => anim.SetBool("isTalking", state);
    public void SetAngry(bool state) => anim.SetBool("isAngry", state);
   public void TriggerGood()
    {
        anim.SetTrigger("Good");
        StartCoroutine(PauseMovementDuringReaction());
    }

    public void TriggerBad()
    {
        anim.SetTrigger("Bad");
        StartCoroutine(PauseMovementDuringReaction());
    }

    private System.Collections.IEnumerator PauseMovementDuringReaction()
    {
        ClientMovement mover = GetComponent<ClientMovement>();
        
        if (mover != null)
        {
            // 1. Aturem el moviment
            mover.SetMovementEnabled(false);
            
            // 2. Esperem el temps que duri l'animació (ajusta els segons segons el teu clip)
            yield return new WaitForSeconds(2.0f); 
            
            // 3. Tornem a activar el moviment per a què pugui marxar
            mover.SetMovementEnabled(true);
        }
    }
}