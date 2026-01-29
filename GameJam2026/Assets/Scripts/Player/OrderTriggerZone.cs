using UnityEngine;

public class OrderTriggerZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // En tutorial, no usar el OrderSystem
            if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Tutorial)
                return;
                
            if (OrderSystem.Instance != null)
                OrderSystem.Instance.SetPlayerAtCounter(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // En tutorial, no usar el OrderSystem
            if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Tutorial)
                return;
                
            if (OrderSystem.Instance != null)
                OrderSystem.Instance.SetPlayerAtCounter(false);
        }
    }
}