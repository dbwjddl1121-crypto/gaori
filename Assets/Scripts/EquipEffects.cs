// 어느 스크립트에서든 장비 효과를 물어볼 수 있는 도우미
// 오브젝트에 붙일 필요 없이 스크립트 파일만 만들면 돼요.
public static class EquipEffects
{
    // 아이템 이름 (상점 ShopItemData의 Item Name과 똑같이)
    public const string OxygenTankName = "산소통";
    public const string HarpoonName = "작살";
    public const string FlashlightName = "손전등";

    // 산소통을 장착하면 늘어나는 산소 시간(초) ← 원하는 숫자로 바꾸세요
    public const float OxygenBonusSeconds = 30f;

    // 산소통을 장착했는지
    public static bool HasOxygenTank
    {
        get { return EquipSlot.IsEquipped(OxygenTankName); }
    }

    // 작살을 장착했는지 (장착하면 물고기를 한 번 클릭으로 잡음)
    public static bool HasHarpoon
    {
        get { return EquipSlot.IsEquipped(HarpoonName); }
    }

    // 손전등을 장착했는지 (나중에 심해에서 사용)
    public static bool HasFlashlight
    {
        get { return EquipSlot.IsEquipped(FlashlightName); }
    }

    // 지금 더해줄 산소 시간 (산소통 없으면 0)
    public static float OxygenBonus
    {
        get { return HasOxygenTank ? OxygenBonusSeconds : 0f; }
    }
}