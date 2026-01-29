using UnityEngine;
using System.Collections;
using TMPro;

public class DogBehaviour : MonoBehaviour
{
    [Header("Animation")]
    public Animator animator;

    [Header("Movement")]
    public Transform offScreenPosition; // Posición fuera de escena (punto inicial)
    public float moveSpeed = 3f;
    public Transform talkingPosition; // Posición donde habla sobre las muertes
    public Transform newActivitiesPosition; // Posición donde habla sobre nuevas actividades
    
    [Header("Waypoints for L Movement")]
    public Transform[] deathWaypoints; // Waypoints para ir a hablar de muertes (movimiento en L)
    public Transform[] activitiesWaypoints; // Waypoints para ir a hablar de actividades (movimiento en L)

    [Header("UI References")]
    public GameObject dialoguePanel; // Panel que contiene el texto
    public TextMeshProUGUI dialogueText;
    public float dialogueDuration = 5f;

    [Header("3D Audio")]
    [SerializeField] private AudioSource idleAudioSource;
    [SerializeField] private AudioSource talkAudioSource;
    
    private string[] deathMessages = new string[]
    {
        "¡Uyuyuy! Se te han muerto {0} clientes. ¡Tienes que mejorar!",
        "¡Vaya desastre! {0} clientes no sobrevivieron. ¡Concéntrate más!",
        "¡Esto es inaceptable! {0} muertes en tu turno. ¡Hazlo mejor!",
        "¡Qué tragedia! Perdiste {0} clientes. ¡Más cuidado la próxima vez!",
        "¡Ay no! {0} clientes fallecieron. ¡Necesitas ser más rápido!",
        "¡Houston, tenemos un problema! {0} bajas. ¡Mejora tu técnica!",
        "¡Catástrofe! {0} clientes no lo lograron. ¡Pon más atención!"
    };

    private string[] newActivitiesMessages = new string[]
    {
        "¡Oye! Hay nuevas actividades disponibles en el manual. ¡Revísalo antes de que vengan los clientes!",
        "¡Atención! Se desbloquearon nuevas actividades. ¡Consulta el manual ahora!",
        "¡Novedad! Nuevas actividades en el manual. ¡Échale un vistazo rápido!",
        "¡Hey! El manual tiene nuevas actividades. ¡Míralo antes de empezar!"
    };

    [Header("Death System")]
    public float deathCheckDelay = 10f;
    private int pendingDeaths = 0;
    private bool isWaitingForDeaths = false;
    private Coroutine deathCheckCoroutine;

    [Header("Activity Check")]
    public float dayStartCheckDelay = 2f;
    private int lastKnownActivityCount = 0;

    private bool isMoving = false;
    private bool isTalking = false;

    void Start()
    {
        // Desactivar el panel al inicio
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
        
        if (dialogueText != null)
        {
            dialogueText.gameObject.SetActive(false);
        }

        if (offScreenPosition != null)
        {
            transform.position = offScreenPosition.position;
        }
    }

    #region Death Notification System
    
    public void NotifyClientDeath()
    {
        pendingDeaths++;
        Debug.Log($"🐕 Muerte registrada. Total pendiente: {pendingDeaths}");

        if (!isWaitingForDeaths)
        {
            if (deathCheckCoroutine != null)
                StopCoroutine(deathCheckCoroutine);
            
            deathCheckCoroutine = StartCoroutine(DeathCheckRoutine());
        }
    }

    private IEnumerator DeathCheckRoutine()
    {
        isWaitingForDeaths = true;
        yield return new WaitForSeconds(deathCheckDelay);

        if (pendingDeaths > 0)
        {
            int deathsToReport = pendingDeaths;
            pendingDeaths = 0;
            
            Debug.Log($"🐕 Reportando {deathsToReport} muerte(s)");
            
            // ACTIVAR el panel antes de moverse
            if (dialoguePanel != null)
            {
                dialoguePanel.SetActive(true);
            }
            
            // Mover en L usando waypoints hacia talkingPosition
            if (deathWaypoints != null && deathWaypoints.Length > 0)
            {
                yield return StartCoroutine(MoveAlongWaypoints(deathWaypoints));
            }
            else if (talkingPosition != null)
            {
                yield return StartCoroutine(MoveToPosition(talkingPosition.position));
            }

            // Mostrar mensaje
            ShowDeathMessage(deathsToReport);
            yield return new WaitForSeconds(dialogueDuration);
            HideDialogue();

            // Volver a offScreen usando los waypoints en reversa
            if (deathWaypoints != null && deathWaypoints.Length > 0)
            {
                yield return StartCoroutine(MoveAlongWaypointsReverse(deathWaypoints));
            }
            else if (offScreenPosition != null)
            {
                yield return StartCoroutine(MoveToPosition(offScreenPosition.position));
            }
            
            // DESACTIVAR el panel después de volver
            if (dialoguePanel != null)
            {
                dialoguePanel.SetActive(false);
            }
        }

        isWaitingForDeaths = false;
        deathCheckCoroutine = null;
    }

    private void ShowDeathMessage(int deathCount)
    {
        if (dialogueText == null) return;

        string randomMessage = deathMessages[Random.Range(0, deathMessages.Length)];
        string finalMessage = string.Format(randomMessage, deathCount);

        dialogueText.text = finalMessage;
        dialogueText.gameObject.SetActive(true);
        
        isTalking = true;
        if (animator != null)
        {
            PlayDogSound(talkAudioSource);
            animator.SetBool("isTalking", true);
        }
    }

    #endregion

    #region New Activities Notification

    public void CheckNewActivitiesOnDayStart(int currentActivityCount)
    {
        StartCoroutine(CheckNewActivitiesRoutine(currentActivityCount));
    }

    private IEnumerator CheckNewActivitiesRoutine(int currentActivityCount)
    {
        yield return new WaitForSeconds(dayStartCheckDelay);

        if (currentActivityCount > lastKnownActivityCount && lastKnownActivityCount > 0)
        {
            // ACTIVAR el panel antes de moverse
            if (dialoguePanel != null)
            {
                dialoguePanel.SetActive(true);
            }
            
            // Mover en L usando waypoints hacia newActivitiesPosition
            if (activitiesWaypoints != null && activitiesWaypoints.Length > 0)
            {
                

                yield return StartCoroutine(MoveAlongWaypoints(activitiesWaypoints));
            }
            else if (newActivitiesPosition != null)
            {
                yield return StartCoroutine(MoveToPosition(newActivitiesPosition.position));
            }

            ShowNewActivitiesMessage();
            yield return new WaitForSeconds(dialogueDuration);
            HideDialogue();
            

            // Volver a offScreen usando los waypoints en reversa
            if (activitiesWaypoints != null && activitiesWaypoints.Length > 0)
            {
                yield return StartCoroutine(MoveAlongWaypointsReverse(activitiesWaypoints));
            }
            else if (offScreenPosition != null)
            {
                yield return StartCoroutine(MoveToPosition(offScreenPosition.position));
            }
            
            // DESACTIVAR el panel después de volver
            if (dialoguePanel != null)
            {
                dialoguePanel.SetActive(false);
            }
        }

        lastKnownActivityCount = currentActivityCount;
    }

    private void ShowNewActivitiesMessage()
    {
        if (dialogueText == null) return;

        string randomMessage = newActivitiesMessages[Random.Range(0, newActivitiesMessages.Length)];
        
        dialogueText.text = randomMessage;
        dialogueText.gameObject.SetActive(true);
        
        isTalking = true;
        if (animator != null)
        {
            PlayDogSound(talkAudioSource);
            animator.SetBool("isTalking", true);
        }
    }

    #endregion

    #region Movement

    private IEnumerator MoveToPosition(Vector3 targetPosition)
    {
        isMoving = true;
        if (animator != null)
        {
            animator.SetBool("isFlying", true);
        }
        
        while (Vector3.Distance(transform.position, targetPosition) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            
            Vector3 direction = (targetPosition - transform.position).normalized;
            if (direction != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
            }
            
            yield return null;
        }

        isMoving = false;
        if (animator != null)
        {
            animator.SetBool("isFlying", false);
        }
    }

    private IEnumerator MoveAlongWaypoints(Transform[] waypoints)
    {
        isMoving = true;
        PlayDogSound(idleAudioSource);
        if (animator != null)
        {
            animator.SetBool("isFlying", true);
        }

        foreach (Transform waypoint in waypoints)
        {
            if (waypoint == null) continue;

            while (Vector3.Distance(transform.position, waypoint.position) > 0.1f)
            {
                transform.position = Vector3.MoveTowards(transform.position, waypoint.position, moveSpeed * Time.deltaTime);
                
                Vector3 direction = (waypoint.position - transform.position).normalized;
                if (direction != Vector3.zero)
                {
                    Quaternion lookRotation = Quaternion.LookRotation(direction);
                    transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
                }
                
                yield return null;
            }
        }

        isMoving = false;
        if (animator != null)
        {
            animator.SetBool("isFlying", false);
        }
    }

    private IEnumerator MoveAlongWaypointsReverse(Transform[] waypoints)
    {
        isMoving = true;
        if (animator != null)
        {
            animator.SetBool("isFlying", true);
        }

        for (int i = waypoints.Length - 1; i >= 0; i--)
        {
            Transform waypoint = waypoints[i];
            if (waypoint == null) continue;

            while (Vector3.Distance(transform.position, waypoint.position) > 0.1f)
            {
                transform.position = Vector3.MoveTowards(transform.position, waypoint.position, moveSpeed * Time.deltaTime);
                
                Vector3 direction = (waypoint.position - transform.position).normalized;
                if (direction != Vector3.zero)
                {
                    Quaternion lookRotation = Quaternion.LookRotation(direction);
                    transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
                }
                
                yield return null;
            }
        }

        isMoving = false;
        StopDogSound(idleAudioSource);
        if (animator != null)
        {
            animator.SetBool("isFlying", false);
        }
    }

    #endregion

    #region Sound Setup
    public void PlayDogSound(AudioSource source)
    {
        if (source == null) source = GetComponent<AudioSource>();

        // Si ja està sonant, no fem res
        if (source.isPlaying) return;

        if (AudioManager.Instance != null)
        {
            // Agafem el clip de la llista centralitzada
            AudioClip clip = null;
            if(source == idleAudioSource) {clip = AudioManager.Instance.GetSFXClip(SFXType.DogIdle);}
            else {clip = AudioManager.Instance.GetSFXClip(SFXType.DogTalk);}
            
            if (clip != null)
            {
                source.clip = clip;
                source.loop = true;
                source.Play();
            }
        }
    }

    // Mètode per aturar el so manualment
    public void StopDogSound(AudioSource source)
    {
        if (source != null && source.isPlaying)
        {
            source.Stop();
            source.loop = false;
        }
    }
    #endregion
    private void HideDialogue()
    {
        if (dialogueText != null)
            dialogueText.gameObject.SetActive(false);
        
        isTalking = false;
        StopDogSound(talkAudioSource);
        if (animator != null)
        {
            animator.SetBool("isTalking", false);
        }
    }
}
