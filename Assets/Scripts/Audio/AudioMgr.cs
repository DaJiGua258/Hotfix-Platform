using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 极简 2D 音效管理器
/// 自动创建 GameObject，挂 AudioSource，只播一次不循环
/// </summary>
public class AudioMgr : SingletonAutoMono<AudioMgr>
{
    private AudioSource _source;
    private Dictionary<string, AudioClip> _cache = new Dictionary<string, AudioClip>();

    void Awake()
    {
        _source = gameObject.AddComponent<AudioSource>();
        _source.spatialBlend = 0f;  // 2D
        _source.playOnAwake = false;
    }

    /// <summary>
    /// 播放音效，路径相对于 Resources/
    /// 例: PlaySFX("Audio/click")  → Resources.Load<AudioClip>("Audio/click")
    /// </summary>
    public void PlaySFX(string path, float volumeScale = 1f)
    {
        if (!_cache.TryGetValue(path, out AudioClip clip))
        {
            clip = Resources.Load<AudioClip>(path);
            if (clip == null)
            {
                Debug.LogWarning($"[AudioMgr] 找不到音效: {path}");
                return;
            }
            _cache[path] = clip;
        }

        _source.PlayOneShot(clip, volumeScale);
    }
}
