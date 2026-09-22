using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Inventory : MonoBehaviour
{
    [Header("인벤토리 UI 패널")]
    public GameObject inventoryPanel; // InventoryPanel 연결

    [Header("하단 닫기 버튼 (BottomBar 내부 버튼)")]
    public Button closeButton; // BottomBar에 있는 닫기 버튼 연결 (선택사항)

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
        // 1. 인벤토리가 닫혀있을 때만: I 키 또는 마우스 우클릭으로 열기
        if (!isOpen)
        {
            if (Input.GetKeyDown(KeyCode.I) || Input.GetMouseButtonDown(1))
            {
                OpenInventory();
            }
        }
        // 2. 인벤토리가 열려있을 때만: ESC 키로 닫기
        else
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                CloseInventory();
            }
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

    // 인벤토리를 닫는 함수 (ESC 또는 하단 바 전용)
    public void CloseInventory()
    {
        isOpen = false;
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
        }
    }
}