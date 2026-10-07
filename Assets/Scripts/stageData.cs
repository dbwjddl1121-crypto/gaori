using UnityEngine;

[CreateAssetMenu(fileName = "stageData", menuName = "Game/Stage Data")]
public class stageData : ScriptableObject
{
    public string stageLabel;      // 예: 스테이지 1
    public string stageTitle;      // 예: 연안
    public Sprite previewImage;    // 선택창에 보이는 바다 이미지
    public int stageIndex;         // 0 = 연안, 1 = 중층, 2 = 심해
}