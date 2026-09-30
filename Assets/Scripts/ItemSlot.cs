using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemSlot : MonoBehaviour
{
    public Image icon;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI priceText;
    public Button buyButton;

    private ShopItemData data;

    public void Setup(ShopItemData item)
    {
        data = item;
        nameText.text = item.itemName;
        priceText.text = item.price + " G";

        if (item.icon != null)
            icon.sprite = item.icon;

        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(OnBuy);
    }

    void OnBuy()
    {
        ShopManager.Instance.TryBuy(data);
    }
}