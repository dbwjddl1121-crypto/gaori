using UnityEngine;

public class Fish : MonoBehaviour
{
    public int clickCount = 0;
    public int clicksNeeded = 3;
    public int cashReward = 100;

    void OnMouseDown()
    {
        clickCount++;
        Debug.Log(gameObject.name + " 클릭 횟수: " + clickCount);

        if (clickCount >= clicksNeeded)
        {
            CashManager.Instance.AddCash(cashReward);
            clickCount = 0;

            // 잡은 물고기를 사라지게 하고 싶으면 아래 주석 해제
            gameObject.SetActive(false);
        }
    }
}
