using UnityEngine;
using UnityEngine.UI;

public class ShopPanelUI : MonoBehaviour
{
    public Button closeButton; // Inspector에서 연결

    void Start()
    {
        closeButton.onClick.AddListener(CloseShop);
    }

    void CloseShop()
    {
        gameObject.SetActive(false);
    }
}