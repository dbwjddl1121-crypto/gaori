using System;
using UnityEngine;

public static class PlayerVitals
{
    // -1이면 저장된 값 없음 (처음 입장)
    public static float savedHealth = -1f;
    public static float maxHealth = -1f;

    // 체력이 외부(포션 등)에서 바뀌었을 때 알림
    public static event Action OnHealthChanged;

    // 회복 성공하면 true (가득 찼거나 사망 상태면 false → 포션 소모 안 함)
    public static bool Heal(float amount)
    {
        if (maxHealth < 0f || savedHealth <= 0f) return false;
        if (savedHealth >= maxHealth) return false;

        savedHealth = Mathf.Min(savedHealth + amount, maxHealth);
        OnHealthChanged?.Invoke();
        return true;
    }
}