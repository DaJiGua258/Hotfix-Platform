using UnityEngine;

/// <summary>
/// 2D 音效管理器
/// 自动创建 GameObject，挂 AudioSource，只播一次不循环
/// 可在 Inspector 面板中拖拽音效资源，未拖拽时自动从 Resources/ 加载
/// </summary>
public class AudioMgr : SingletonAutoMono<AudioMgr>
{
    [Header("音效资源（拖拽或放到 Resources/Audio/ 下）")]
    [SerializeField] private AudioClip footstepClip;
    [SerializeField] private AudioClip jumpClip;
    [SerializeField] private AudioClip coinClip;
    [SerializeField] private AudioClip landClip;

    private AudioSource _source;       // 一次性音效（跳跃、金币）
    private AudioSource _loopSource;    // 循环音效（脚步声）
    private bool _wasPaused;            // 上一帧是否暂停

    void Awake()
    {
        _source = gameObject.AddComponent<AudioSource>();
        _source.spatialBlend = 0f;
        _source.playOnAwake = false;

        _loopSource = gameObject.AddComponent<AudioSource>();
        _loopSource.spatialBlend = 0f;
        _loopSource.playOnAwake = false;
        _loopSource.loop = true;

        // 未拖拽时从 Resources 加载
        if (footstepClip == null) footstepClip = Resources.Load<AudioClip>("Audio/walking");
        if (jumpClip == null) jumpClip = Resources.Load<AudioClip>("Audio/jump");
        if (coinClip == null) coinClip = Resources.Load<AudioClip>("Audio/coin");
        if (landClip == null) landClip = Resources.Load<AudioClip>("Audio/land");
    }

    void Update()
    {
        if (Time.timeScale == 0)
        {
            _loopSource.Stop();
            _wasPaused = true;
        }
        else if (_wasPaused)
        {
            _wasPaused = false;
        }
    }

    /// <summary>
    /// 播放指定 AudioClip
    /// </summary>
    public void PlayOneShot(AudioClip clip, float volumeScale = 1f)
    {
        if (clip != null)
            _source.PlayOneShot(clip, volumeScale);
    }

    /// <summary>
    /// 开始循环播放脚步声
    /// </summary>
    public void StartFootstep(float volumeScale = 1f)
    {
        if (footstepClip == null) return;
        _loopSource.clip = footstepClip;
        _loopSource.volume = volumeScale;
        _loopSource.Play();
    }

    /// <summary>
    /// 停止循环播放脚步声
    /// </summary>
    public void StopFootstep()
    {
        _loopSource.Stop();
    }

    /// <summary>
    /// 播放跳跃音效
    /// </summary>
    public void PlayJump(float volumeScale = 1f)
    {
        if (jumpClip != null)
            _source.PlayOneShot(jumpClip, volumeScale);
    }

    /// <summary>
    /// 播放金币音效
    /// </summary>
    public void PlayCoin(float volumeScale = 1f)
    {
        if (coinClip != null)
            _source.PlayOneShot(coinClip, volumeScale);
    }

    /// <summary>
    /// 播放落地音效
    /// </summary>
    public void PlayLand(float volumeScale = 1f)
    {
        if (landClip != null)
            _source.PlayOneShot(landClip, volumeScale);
    }
}
