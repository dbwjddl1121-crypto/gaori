using UnityEngine;

[RequireComponent(typeof(PlayerMove))]
public class PlayerFootstepSound : MonoBehaviour
{
    [Header("걸을 때 반복해서 틀 소리")]
    public AudioClip footstepClip;

    [Range(0f, 1f)]
    public float volume = 0.5f;   // 이 소리만의 크기 (효과음 슬라이더도 같이 반영됨)

    private AudioSource src;
    private PlayerMove playerMove;

    void Awake()
    {
        playerMove = GetComponent<PlayerMove>();

        src = gameObject.AddComponent<AudioSource>();
        src.clip = footstepClip;
        src.loop = true;
        src.playOnAwake = false;
        src.spatialBlend = 0f;   // 2D 소리
    }

    void Update()
    {
        if (footstepClip == null) return;

        // 조작할 수 있는 상태에서 이동 키를 누르고 있을 때만 소리
        bool moving = Time.timeScale > 0f
                      && playerMove != null && playerMove.enabled
                      && (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0);

        // 설정창의 효과음 슬라이더 값을 따라감
        src.volume = volume * SettingsManager.SFXVolume;

        if (moving && !src.isPlaying)
            src.Play();
        else if (!moving && src.isPlaying)
            src.Stop();
    }
}