using UnityEngine;

public class OrderTriggerZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) 
            OrderSystem.Instance.SetPlayerAtCounter(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) 
            OrderSystem.Instance.SetPlayerAtCounter(false);
    }
}