using System.Collections.Generic;
using UnityEngine;

// 인벤토리 저장소
// 게임을 완전히 껐다 켜도 유지됩니다 (아이템 이름과 개수를 PlayerPrefs에 저장)
public static class InventoryStore
{
    const string SaveKey = "InventorySlots";

    private static List<Item> current = new List<Item>();
    private static List<Item> checkpoint = new List<Item>();
    private static bool loaded = false;

    // 아이템 이름 → 아이콘 (불러올 때 아이콘을 다시 붙이기 위한 표)
    private static Dictionary<string, Sprite> iconCache;

    // 복원/초기화 직후 씬이 사라지면서 덮어쓰는 것을 막는 잠금
    public static bool lockSave = false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics()
    {
        current = new List<Item>();
        checkpoint = new List<Item>();
        loaded = false;
        iconCache = null;
        lockSave = false;
    }

    // ───────── 복사 ─────────

    static Item Copy(Item src)
    {
        if (src == null) return null;
        Item c = new Item();
        c.itemName = src.itemName;
        c.itemIcon = src.itemIcon;
        c.itemCount = src.itemCount;
        return c;
    }

    static List<Item> CopyList(List<Item> src)
    {
        List<Item> result = new List<Item>();
        foreach (Item i in src) result.Add(Copy(i));
        return result;
    }

    static int CountItems(List<Item> list)
    {
        int n = 0;
        foreach (Item i in list) if (i != null) n++;
        return n;
    }

    // ───────── 아이콘 찾기 ─────────

    static void BuildIconCache()
    {
        iconCache = new Dictionary<string, Sprite>();

        // 쓰레기 아이콘 (Resources/TrashDatabase)
        TrashDatabase db = TrashDatabase.Instance;
        if (db != null)
        {
            foreach (TrashEntry e in db.entries)
            {
                if (e != null && !string.IsNullOrEmpty(e.itemName) && e.icon != null)
                    iconCache[e.itemName] = e.icon;
            }
        }

        // 상점 아이템 아이콘 (Resources 폴더 안의 ShopItemData)
        ShopItemData[] shopItems = Resources.LoadAll<ShopItemData>("");
        foreach (ShopItemData s in shopItems)
        {
            if (s != null && !string.IsNullOrEmpty(s.itemName) && s.icon != null)
                iconCache[s.itemName] = s.icon;
        }
    }

    static Sprite FindIcon(string itemName)
    {
        if (string.IsNullOrEmpty(itemName)) return null;
        if (iconCache == null) BuildIconCache();

        Sprite icon;
        return iconCache.TryGetValue(itemName, out icon) ? icon : null;
    }

    // ───────── 저장 / 불러오기 (PlayerPrefs) ─────────

    static string Serialize(List<Item> items)
    {
        List<string> lines = new List<string>();

        foreach (Item i in items)
            lines.Add(i == null ? "" : i.itemName + "\t" + i.itemCount);

        return string.Join("\n", lines.ToArray());
    }

    static List<Item> Deserialize(string raw)
    {
        List<Item> result = new List<Item>();
        if (string.IsNullOrEmpty(raw)) return result;

        foreach (string line in raw.Split('\n'))
        {
            if (string.IsNullOrEmpty(line))
            {
                result.Add(null);   // 빈 칸
                continue;
            }

            string[] parts = line.Split('\t');

            int count = 1;
            if (parts.Length > 1 && !int.TryParse(parts[1], out count))
                count = 1;

            Item item = new Item();
            item.itemName = parts[0];
            item.itemCount = count;
            item.itemIcon = FindIcon(parts[0]);

            if (item.itemIcon == null)
                Debug.LogWarning("[InventoryStore] 아이콘을 찾지 못했어요 → " + parts[0]);

            result.Add(item);
        }

        return result;
    }

    static void WriteToPrefs()
    {
        PlayerPrefs.SetString(SaveKey, Serialize(current));
        PlayerPrefs.Save();
    }

    // 처음 쓸 때 저장된 인벤토리를 한 번만 불러옴
    static void EnsureLoaded()
    {
        if (loaded) return;
        loaded = true;

        current = Deserialize(PlayerPrefs.GetString(SaveKey, ""));
        Debug.Log("[InventoryStore] 저장된 인벤토리를 불러왔어요 (아이템 " + CountItems(current) + "개)");
    }

    // ───────── 기존 기능 ─────────

    // 현재 슬롯 상태를 저장 (슬롯 순서대로, 빈 칸은 null)
    public static void Save(List<Item> slotItems)
    {
        if (lockSave)
        {
            Debug.Log("[InventoryStore] 저장이 막혀 있어서 건너뜀 (lockSave)");
            return;
        }

        loaded = true;
        current = CopyList(slotItems);
        WriteToPrefs();
        Debug.Log("[InventoryStore] 저장함 (아이템 " + CountItems(current) + "개)");
    }

    public static List<Item> Load()
    {
        EnsureLoaded();
        Debug.Log("[InventoryStore] Load 호출 (아이템 " + CountItems(current) + "개)");
        return CopyList(current);
    }

    // 바다에 들어온 순간 호출
    public static void SaveCheckpoint()
    {
        EnsureLoaded();
        checkpoint = CopyList(current);
    }

    // 죽고 다시 시작할 때 호출
    public static void RestoreCheckpoint()
    {
        EnsureLoaded();
        current = CopyList(checkpoint);
        WriteToPrefs();
        lockSave = true;
        Debug.Log("[InventoryStore] 체크포인트로 되돌렸어요 (아이템 " + CountItems(current) + "개)");
    }

    // 타이틀로 갈 때 호출: 완전 초기화 (저장된 인벤토리도 함께 지움)
    public static void ResetAll()
    {
        loaded = true;
        current.Clear();
        checkpoint.Clear();

        PlayerPrefs.DeleteKey(SaveKey);
        PlayerPrefs.Save();

        lockSave = true;

        // 이 글씨를 클릭하면 Console 아래쪽에 누가 불렀는지(스크립트 이름) 나와요
        Debug.LogWarning("[InventoryStore] ResetAll 호출됨 → 저장된 인벤토리를 지웠어요!");
    }
}