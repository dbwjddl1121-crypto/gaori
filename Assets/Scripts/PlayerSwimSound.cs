using UnityEngine;

public class PlayerSwimSound : MonoBehaviour
{
    [Header("이동할 때 반복해서 틀 소리")]
    public AudioClip swimClip;

    [Range(0f, 1f)]
    public float volume = 0.6f;   // 이 소리만의 크기 (효과음 슬라이더도 같이 반영됨)

    private AudioSource src;

    void Awake()
    {
        src = gameObject.AddComponent<AudioSource>();
        src.clip = swimClip;
        src.loop = true;
        src.playOnAwake = false;
        src.spatialBlend = 0f;   // 2D 소리
    }

    void Update()
    {
        if (swimClip == null) return;

        // 설정창이 열려 멈춘 상태(Time.timeScale = 0)에서는 소리도 멈춤
        bool moving = Time.timeScale > 0f &&
                      (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0);

        // 설정창의 효과음 슬라이더 값을 따라감
        src.volume = volume * SettingsManager.SFXVolume;

        if (moving && !src.isPlaying)
            src.Play();
        else if (!moving && src.isPlaying)
            src.Stop();
    }
}