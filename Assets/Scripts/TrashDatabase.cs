using System.Collections.Generic;
using UnityEngine;

// 분리배출 분류 (작업장 쓰레기통 5개와 같게 맞춤)
public enum TrashCategory
{
    일반류,
    유리병,
    플라스틱,
    비닐류,
    캔류,
    기타
}

[System.Serializable]
public class TrashEntry
{
    public string itemName;                 // 아이템 이름 (코드의 itemName과 똑같이!)
    public Sprite icon;                     // 도감에 보일 아이콘
    public TrashCategory category;          // 어느 쓰레기통에 버리는지

    [TextArea(2, 4)] public string description;   // 설명창에 뜨는 설명
    [TextArea(2, 4)] public string recycleTip;    // 올바른 분리배출 방법
}

[CreateAssetMenu(fileName = "TrashDatabase", menuName = "Recycling/Trash Database")]
public class TrashDatabase : ScriptableObject
{
    public List<TrashEntry> entries = new List<TrashEntry>();

    private static TrashDatabase instance;

    // Assets/Resources/TrashDatabase 를 자동으로 불러옴
    public static TrashDatabase Instance
    {
        get
        {
            if (instance == null)
                instance = Resources.Load<TrashDatabase>("TrashDatabase");
            return instance;
        }
    }

    public TrashEntry Find(string itemName)
    {
        if (string.IsNullOrEmpty(itemName)) return null;
        foreach (TrashEntry e in entries)
            if (e.itemName == itemName) return e;
        return null;
    }
}