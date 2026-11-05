using UnityEngine;

[CreateAssetMenu(fileName = "stageData", menuName = "Game/Stage Data")]
public class stageData : ScriptableObject
{
    [Header("한국어")]
    public string stageLabel;      // 예: 스테이지 1
    public string stageTitle;      // 예: 연안

    [TextArea(3, 8)]
    public string stageDescription; // 스테이지 설명 글

    [Header("영어")]
    public string stageLabelEn;      // 예: Stage 1
    public string stageTitleEn;      // 예: Coastal Zone

    [TextArea(3, 8)]
    public string stageDescriptionEn; // 스테이지 설명 글 (영어)

    [Header("공통")]
    public Sprite previewImage;    // 선택창에 보이는 바다 이미지
    public int stageIndex;         // 0 = 연안, 1 = 중층, 2 = 심해
}
