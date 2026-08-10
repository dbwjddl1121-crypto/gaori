using UnityEngine;

public class TrashInteraction : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("쓰레기와 상호작용 가능!");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("쓰레기에서 멀어짐");
        }
    }
}