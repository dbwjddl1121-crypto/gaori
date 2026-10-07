using System.Collections.Generic;

public static class InventoryStore
{
    private static List<Item> current = new List<Item>();
    private static List<Item> checkpoint = new List<Item>();

    // 복원/초기화 직후 씬이 사라지면서 덮어쓰는 것을 막는 잠금
    public static bool lockSave = false;

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

    // 현재 슬롯 상태를 저장 (슬롯 순서대로, 빈 칸은 null)
    public static void Save(List<Item> slotItems)
    {
        if (lockSave) return;
        current = CopyList(slotItems);
    }

    public static List<Item> Load()
    {
        return CopyList(current);
    }

    // 바다에 들어온 순간 호출
    public static void SaveCheckpoint()
    {
        checkpoint = CopyList(current);
    }

    // 죽고 다시 시작할 때 호출
    public static void RestoreCheckpoint()
    {
        current = CopyList(checkpoint);
        lockSave = true;
    }

    // 타이틀로 갈 때 호출: 완전 초기화
    public static void ResetAll()
    {
        current.Clear();
        checkpoint.Clear();
        lockSave = true;
    }
}