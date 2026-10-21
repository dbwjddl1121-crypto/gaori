using System.Collections.Generic;

// 아이템 설명 조회: TrashDatabase에 있으면 거기서, 없으면 아래 예비 표에서 찾음
public static class ItemDescriptions
{
    // 쓰레기가 아닌 아이템(포션 등)용 예비 표
    private static readonly Dictionary<string, string> fallback = new Dictionary<string, string>
    {
        { "체력 포션", "마시면 체력을 15 회복한다." },
    };

    public static string Get(string itemName)
    {
        TrashDatabase db = TrashDatabase.Instance;
        if (db != null)
        {
            TrashEntry e = db.Find(itemName);
            if (e != null)
            {
                string text = "[" + e.category + "] " + e.description;
                if (!string.IsNullOrEmpty(e.recycleTip))
                    text += "\n" + e.recycleTip;
                return text;
            }
        }

        if (itemName != null && fallback.TryGetValue(itemName, out string desc))
            return desc;

        return "설명이 없습니다.";
    }
}