using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonSound : MonoBehaviour
{
    [Tooltip("AudioManager의 Sfx List에 적은 이름")]
    public string sfxName = "click";

    void Awake()
    {
        GetComponent<Button>().onClick.AddListener(Play);
    }

    void Play()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(sfxName);
        }
    }
}