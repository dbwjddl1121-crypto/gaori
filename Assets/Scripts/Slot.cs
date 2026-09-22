using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Slot : MonoBehaviour
{
    public Image iconImage;
    public TextMeshProUGUI countText;

    private Item currentItem;
    public Item CurrentItem => currentItem;

    public void SetItem(Item item)
    {
        currentItem = item;

        if (iconImage != null && item.itemIcon != null)
        {
            iconImage.sprite = item.itemIcon;
            iconImage.gameObject.SetActive(true);

            // 알파값을 확실하게 불투명(1)으로 설정
            Color color = iconImage.color;
            color.a = 1f;
            iconImage.color = color;

            Debug.Log("슬롯에 아이콘 세팅 완료: " + item.itemIcon.name);
        }
        else
        {
            Debug.LogWarning("Slot: iconImage가 비어있거나 item.itemIcon이 null입니다!");
        }

        if (countText != null)
        {
            countText.text = item.itemCount > 1 ? item.itemCount.ToString() : "";
        }
    }

    public void ClearSlot()
    {
        currentItem = null;
        if (iconImage != null)
        {
            iconImage.sprite = null;
            iconImage.gameObject.SetActive(false);
        }
        if (countText != null)
        {
            countText.text = "";
        }
    }
}