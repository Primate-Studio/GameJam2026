using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class ClientAnimationController : MonoBehaviour
{
    [SerializeField] private AudioSource talkingAudioSource;
    private Animator anim;
    private NavMeshAgent agent;
    public bool isTalking = false;

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

    public IEnumerator SetTalking(bool state, float duration = 2.0f)
    {
        isTalking = state;
        AudioClip clip = AudioManager.Instance.GetSFXClip(SFXType.ClientTalk);
        if (talkingAudioSource != null && clip != null)
        {
            if (state)
            {
                talkingAudioSource.clip = clip;
                talkingAudioSource.loop = true;
                talkingAudioSource.Play();
            }
            else
            {
                talkingAudioSource.Stop();
                talkingAudioSource.loop = false;
            }
        }
        anim.SetBool("isTalking", state);
        yield return new WaitForSeconds(duration);
        anim.SetBool("isTalking", false);
        talkingAudioSource.Stop();
        talkingAudioSource.loop = false;
        isTalking = false;
    }

    public void SetAngry(bool state) => anim.SetBool("isAngry", state);
    public void SetIdle(bool state) => anim.SetBool("isIdle", state);
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
    private IEnumerator PauseMovementDuringReaction()
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