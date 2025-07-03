using UnityEngine;

public class FinishFlag : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            GameManager.Instance.UpdateLevelCompleted(true);
        }
    }
}
