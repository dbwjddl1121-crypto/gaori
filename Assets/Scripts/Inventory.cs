using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Inventory : MonoBehaviour
{
    [Header("인벤토리 UI 패널")]
    public GameObject inventoryPanel; // InventoryPanel 연결

    [Header("닫기 버튼 (X 버튼 / EscButton)")]
    public Button closeButton; // 인벤토리의 X(닫기) 버튼 연결 (선택사항)

    private bool isOpen = false;

    void Start()
    {
        // 시작할 때는 인벤토리를 꺼둡니다.
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
        }

        // 닫기 버튼에 클릭 이벤트 연결
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseInventory);
        }
    }

    void Update()
    {
        // E 키: 열려있으면 닫고, 닫혀있으면 연다 (토글)
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (isOpen) CloseInventory();
            else OpenInventory();
        }
    }

    // 인벤토리를 여는 함수
    public void OpenInventory()
    {
        isOpen = true;
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(true);
        }
    }

    // 인벤토리를 닫는 함수 (E 키 또는 X 버튼)
    public void CloseInventory()
    {
        isOpen = false;
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
        }
    }
}