using UnityEngine;
using UnityEngine.UI;

public class ShopDesk : MonoBehaviour
{
    public GameObject shopPanel; // Inspector에서 연결

    void Start()
    {
        GetComponent<Button>().onClick.AddListener(OpenShop);
    }

    void OpenShop()
    {
        shopPanel.SetActive(true);
    }
}