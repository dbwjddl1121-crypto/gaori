using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Slot : MonoBehaviour
{
    [Header("UI 컴포넌트")]
    public Image iconImage;          // 아이템 아이콘 이미지
    public TextMeshProUGUI countText;  // 아이템 개수 텍스트

    private Item currentItem;          // 현재 슬롯에 들어있는 아이템 정보
    public Item CurrentItem => currentItem;

    // 슬롯에 아이템 정보를 세팅하는 함수
    public void SetItem(Item item)
    {
        currentItem = item;

        if (iconImage != null && item != null && item.itemIcon != null)
        {
            // 아이콘 이미지 설정
            iconImage.sprite = item.itemIcon;
            iconImage.gameObject.SetActive(true);

            // 알파값(불투명도)을 1로 확실하게 설정하여 이미지가 투명해지는 현상 방지
            Color color = iconImage.color;
            color.a = 1f;
            iconImage.color = color;
        }
        else
        {
            Debug.LogWarning("Slot: 아이콘을 설정할 수 없습니다. 아이템 또는 이미지가 비어있습니다.");
        }

        // 아이템 개수가 2개 이상일 때만 텍스트 표시, 1개 이하면 숨김
        if (countText != null && item != null)
        {
            countText.text = item.itemCount > 1 ? item.itemCount.ToString() : "";
        }
    }

    // 슬롯을 비우는 함수
    public void ClearSlot()
    {
        currentItem = null;

        if (iconImage != null)
        {
            iconImage.sprite = null;
            iconImage.gameObject.SetActive(false);

            // 다음을 위해 알파값도 원상복구
            Color color = iconImage.color;
            color.a = 1f;
            iconImage.color = color;
        }

        if (countText != null)
        {
            countText.text = "";
        }
    }
}