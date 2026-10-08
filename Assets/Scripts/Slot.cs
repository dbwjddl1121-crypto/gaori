using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class Slot : MonoBehaviour, IPointerClickHandler
{
    [Header("UI 컴포넌트")]
    public Image iconImage;
    public TextMeshProUGUI countText;

    [Header("체력 포션 설정")]
    public string potionName = "체력 포션";  // 상점 아이템 이름과 똑같이
    public float healAmount = 15f;           // 한 번에 회복되는 체력

    private Item currentItem;
    public Item CurrentItem => currentItem;

    public void SetItem(Item item)
    {
        currentItem = item;

        if (iconImage != null && item != null && item.itemIcon != null)
        {
            iconImage.sprite = item.itemIcon;
            iconImage.gameObject.SetActive(true);

            Color color = iconImage.color;
            color.a = 1f;
            iconImage.color = color;
        }
        else
        {
            Debug.LogWarning("Slot: 아이콘을 설정할 수 없습니다. 아이템 또는 이미지가 비어있습니다.");
        }

        if (countText != null && item != null)
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

            Color color = iconImage.color;
            color.a = 1f;
            iconImage.color = color;
        }

        if (countText != null)
        {
            countText.text = "";
        }
    }

    // 슬롯 우클릭 → 체력 포션 사용
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Right) return;
        if (currentItem == null) return;

        Debug.Log("클릭한 아이템 이름: " + currentItem.itemName);
        if (currentItem.itemName != potionName) return;

        // 체력이 가득 차 있으면 사용하지 않음
        if (!PlayerVitals.Heal(healAmount))
        {
            Debug.Log("지금은 체력 포션을 쓸 수 없어요 (체력이 가득 참)");
            return;
        }

        // 포션 1개 소모
        currentItem.itemCount--;
        if (currentItem.itemCount <= 0)
            ClearSlot();
        else
            SetItem(currentItem);

        // 변경된 인벤토리를 저장소에 기록
        InventoryManager manager = FindFirstObjectByType<InventoryManager>();
        if (manager != null) manager.SaveToStore();
    }
}