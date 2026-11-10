using UnityEngine;
using TMPro;
using System.Collections.Generic;

[System.Serializable]
public class EquipInfo
{
    public string itemName;      // 상점 아이템 이름과 똑같이
    public string performance;   // 성능 (예: 물고기 한 번 클릭으로 포획)
    public string damage;        // 데미지
    public string attackSpeed;   // 공격 속도
    public string moveSpeed;     // 이동 속도
}

public class EquipInfoPanel : MonoBehaviour
{
    [Header("연결")]
    public EquipSlot equipSlot;            // 이 정보창이 보여줄 장비칸

    [Header("텍스트 (없는 줄은 비워도 됨)")]
    public TextMeshProUGUI nameText;       // "장비 없음" 글씨
    public TextMeshProUGUI performanceText;
    public TextMeshProUGUI damageText;
    public TextMeshProUGUI attackSpeedText;
    public TextMeshProUGUI moveSpeedText;

    [Header("아이템별 정보")]
    public List<EquipInfo> infos = new List<EquipInfo>();

    [Header("빈 칸일 때 표시")]
    public string emptyName = "장비 없음";

    void Awake()
    {
        if (equipSlot == null)
            equipSlot = GetComponent<EquipSlot>();
    }

    void OnEnable()
    {
        EquipSlot.OnEquipChanged += Refresh;
        Refresh();
    }

    void OnDisable()
    {
        EquipSlot.OnEquipChanged -= Refresh;
    }

    public void Refresh()
    {
        Item item = equipSlot != null ? equipSlot.CurrentItem : null;

        if (item == null)
        {
            Set(nameText, emptyName);
            Set(performanceText, "성능: ");
            Set(damageText, "데미지: ");
            Set(attackSpeedText, "공격 속도: ");
            Set(moveSpeedText, "이동 속도: ");
            return;
        }

        EquipInfo info = Find(item.itemName);

        Set(nameText, item.itemName);
        Set(performanceText, "성능: " + (info != null ? info.performance : ""));
        Set(damageText, "데미지: " + (info != null ? info.damage : ""));
        Set(attackSpeedText, "공격 속도: " + (info != null ? info.attackSpeed : ""));
        Set(moveSpeedText, "이동 속도: " + (info != null ? info.moveSpeed : ""));
    }

    EquipInfo Find(string itemName)
    {
        foreach (EquipInfo info in infos)
        {
            if (info.itemName == itemName)
                return info;
        }
        return null;
    }

    void Set(TextMeshProUGUI text, string value)
    {
        if (text != null)
            text.text = value;
    }
}