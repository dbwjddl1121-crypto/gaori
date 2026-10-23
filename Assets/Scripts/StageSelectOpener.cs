using System.Collections;
using UnityEngine;

// GameScene이 시작될 때, 정화 후 돌아온 경우 스테이지 선택창을 자동으로 연다
public class StageSelectOpener : MonoBehaviour
{
    [SerializeField] GameObject stageSelectPanel;

    IEnumerator Start()
    {
        if (!StageProgress.openStageSelect) yield break;

        // 다른 스크립트의 Start()가 전부 끝난 뒤에 열기 (SceneFader가 패널을 끄는 것 방지)
        yield return null;
        yield return null;

        StageProgress.openStageSelect = false;

        if (stageSelectPanel != null)
            stageSelectPanel.SetActive(true);

        // 패널이 켜져서 OnEnable이 returnStage를 읽은 뒤에 초기화
        yield return null;
        StageProgress.returnStage = -1;
    }
}