using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FishWatcher : MonoBehaviour
{
    [Header("이동할 씬")]
    [SerializeField] string endingSceneName = "EndingScene";

    [Header("마지막 물고기가 사라진 뒤 추가로 기다릴 시간")]
    [Tooltip("물고기가 잡힐 때 0.3초 연출이 이미 있어서 기본은 0이에요.")]
    [SerializeField] float extraDelay = 0f;

    List<PixelFishSmooth> fishList = new List<PixelFishSmooth>();
    bool ready;
    bool triggered;

    IEnumerator Start()
    {
        // 미리보기로 겹쳐 로드된 씬에서는 동작하지 않게 함
        if (gameObject.scene != SceneManager.GetActiveScene())
            yield break;

        // 한 프레임 대기: 이미 잡았던 물고기는 Start에서 삭제되므로, 사라진 뒤에 센다
        yield return null;

        foreach (var f in FindObjectsByType<PixelFishSmooth>(FindObjectsSortMode.None))
        {
            if (f.gameObject.scene == gameObject.scene)
                fishList.Add(f);
        }

        if (fishList.Count == 0)
        {
            // 물고기가 처음부터 없으면 엔딩을 걸지 않음
            Debug.LogWarning("[FishWatcher] 이 씬에서 물고기를 찾지 못했어요.");
            yield break;
        }

        ready = true;
    }

    void Update()
    {
        if (!ready || triggered)
            return;

        // 삭제된(null) 물고기를 빼고 남은 수를 센다
        int remaining = 0;
        foreach (var f in fishList)
        {
            if (f != null)
                remaining++;
        }

        if (remaining == 0)
        {
            triggered = true;
            StartCoroutine(GoBadEnding());
        }
    }

    IEnumerator GoBadEnding()
    {
        if (extraDelay > 0f)
            yield return new WaitForSeconds(extraDelay);

        EndingController.pending = EndingController.EndingType.Bad;
        SceneManager.LoadScene(endingSceneName);
    }
}