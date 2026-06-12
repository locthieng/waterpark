using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundController : MonoBehaviour
{
    //Drag a reference to the audio source which will play the sound effects.
    public AudioSource efxSource1;
    public AudioSource efxSource2;
    public AudioSource efxSource3;
    public AudioSource efxSource4;
    public AudioSource efxSource5;
    public AudioSource efxSource6;
    public AudioSource efxSource7;
    public AudioSource efxSource8;
    public AudioSource efxSource9;
    public AudioSource ExclusiveSource;
    public AudioSource ExclusiveSource1;
    public AudioSource ExclusiveSource2;
    public AudioSource ExclusiveSource3;
    public AudioSource ExclusiveSource4;
    //Drag a reference to the audio source which will play the music.
    public AudioSource musicSource;
    public AudioSource sandSource;
    //Allows other scripts to call functions from SoundManager.             
    public static SoundController Instance { get; set; }
    //The lowest a sound effect will be randomly pitched.
    public float lowPitchRange = 0.95f;
    //The highest a sound effect will be randomly pitched.
    public float highPitchRange = 1.05f;
    public AudioClip StoppedMusic { get; set; }

    [Header("Audio Clips")]
    public AudioClip bg;
    public AudioClip SandFly;
    public AudioClip BottleCap;
    public AudioClip BottleFly;
    public AudioClip winClip;
    public AudioClip loseClip;
    public AudioClip clickClip;
    public AudioClip SfxButton;
    public AudioClip Coin;
    public AudioClip Booster_2;
    public AudioClip Booster_3;

    private void Update()
    {
         
    }

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    public AudioSource PlaySingleExclusive(AudioClip clip, float pitch = 1f, bool loop = false, float volume = 1)
    {
        if (!GlobalController.IsSoundOn || clip == null || (ExclusiveSource.clip == clip && loop)) return null;
        if (ExclusiveSource.isPlaying)
        {
            return PlaySingleExclusive1(clip, pitch, loop, volume);
        }
        else
        {
            ExclusiveSource.clip = clip;
            ExclusiveSource.volume = volume;
            ExclusiveSource.pitch = pitch;
            ExclusiveSource.loop = loop;
            ExclusiveSource.Play();
            return ExclusiveSource;
        }
    }

    private AudioSource PlaySingleExclusive1(AudioClip clip, float pitch = 1f, bool loop = false, float volume = 1)
    {
        if (!GlobalController.IsSoundOn || clip == null || (ExclusiveSource1.clip == clip && loop)) return null;
        if (ExclusiveSource1.isPlaying)
        {
            return PlaySingleExclusive2(clip, pitch, loop, volume);
        }
        else
        {
            ExclusiveSource1.clip = clip;
            ExclusiveSource1.volume = volume;
            ExclusiveSource1.pitch = pitch;
            ExclusiveSource1.loop = loop;
            ExclusiveSource1.Play();
            return ExclusiveSource1;
        }
    }

    private AudioSource PlaySingleExclusive2(AudioClip clip, float pitch = 1f, bool loop = false, float volume = 1)
    {
        if (!GlobalController.IsSoundOn || clip == null || (ExclusiveSource2.clip == clip && loop)) return null;
        if (ExclusiveSource2.isPlaying)
        {
            return PlaySingleExclusive3(clip, pitch, loop, volume);
        }
        else
        {
            ExclusiveSource2.clip = clip;
            ExclusiveSource2.volume = volume;
            ExclusiveSource2.pitch = pitch;
            ExclusiveSource2.loop = loop;
            ExclusiveSource2.Play();
            return ExclusiveSource2;
        }
    }

    private AudioSource PlaySingleExclusive3(AudioClip clip, float pitch = 1f, bool loop = false, float volume = 1)
    {
        if (!GlobalController.IsSoundOn || clip == null || (ExclusiveSource3.clip == clip && loop)) return null;
        if (ExclusiveSource3.isPlaying)
        {
            return PlaySingleExclusive4(clip, pitch, loop, volume);
        }
        else
        {
            ExclusiveSource3.clip = clip;
            ExclusiveSource3.volume = volume;
            ExclusiveSource3.pitch = pitch;
            ExclusiveSource3.loop = loop;
            ExclusiveSource3.Play();
            return ExclusiveSource3;
        }
    }

    private AudioSource PlaySingleExclusive4(AudioClip clip, float pitch = 1f, bool loop = false, float volume = 1)
    {
        if (!GlobalController.IsSoundOn || clip == null || (ExclusiveSource4.clip == clip && loop)) return null;
        ExclusiveSource4.clip = clip;
        ExclusiveSource4.volume = volume;
        ExclusiveSource4.pitch = pitch;
        ExclusiveSource4.loop = loop;
        ExclusiveSource4.Play();
        return ExclusiveSource4;
    }

    // Used to play single sound clips.
    public AudioSource PlaySingle(AudioClip clip, float pitch = 1f, bool loop = false, float volume = 1)
    {
        if (!GlobalController.IsSoundOn || clip == null) return null;
        if (efxSource1.isPlaying)
        {
            return PlaySingle1(clip, pitch, loop, volume);
        }
        else
        {
            efxSource1.clip = clip;
            efxSource1.volume = volume * GlobalController.SoundVolume;
            efxSource1.pitch = pitch;
            efxSource1.loop = loop;
            efxSource1.Play();
            return efxSource1;
        }
    }

    private AudioSource PlaySingle1(AudioClip clip, float pitch = 1f, bool loop = false, float volume = 1)
    {
        if (efxSource2.isPlaying)
        {
            return PlaySingle2(clip, pitch, loop, volume);
        }
        else
        {
            efxSource2.volume = volume * GlobalController.SoundVolume;
            efxSource2.clip = clip;
            efxSource2.loop = loop;
            efxSource2.pitch = pitch;
            efxSource2.Play();
            return efxSource2;
        }
    }

    private AudioSource PlaySingle2(AudioClip clip, float pitch = 1f, bool loop = false, float volume = 1)
    {
        if (efxSource3.isPlaying)
        {
            return PlaySingle3(clip, pitch, loop, volume);
        }
        else
        {
            efxSource3.volume = volume * GlobalController.SoundVolume;
            efxSource3.clip = clip;
            efxSource3.loop = loop;
            efxSource3.pitch = pitch;
            efxSource3.Play();
            return efxSource3;
        }
    }

    private AudioSource PlaySingle3(AudioClip clip, float pitch = 1f, bool loop = false, float volume = 1)
    {
        if (efxSource4.isPlaying)
        {
            return PlaySingle4(clip, pitch, loop, volume);
        }
        else
        {
            efxSource4.volume = volume * GlobalController.SoundVolume;
            efxSource4.clip = clip;
            efxSource4.loop = loop;
            efxSource4.pitch = pitch;
            efxSource4.Play();
            return efxSource4;
        }
    }

    private AudioSource PlaySingle4(AudioClip clip, float pitch = 1f, bool loop = false, float volume = 1)
    {
        if (efxSource5.isPlaying)
        {
            return PlaySingle5(clip, pitch, loop, volume);
        }
        else
        {
            efxSource5.volume = volume * GlobalController.SoundVolume;
            efxSource5.clip = clip;
            efxSource5.loop = loop;
            efxSource5.pitch = pitch;
            efxSource5.Play();
            return efxSource5;
        }
    }

    private AudioSource PlaySingle5(AudioClip clip, float pitch = 1f, bool loop = false, float volume = 1)
    {
        if (efxSource6.isPlaying)
        {
            return PlaySingle6(clip, pitch, loop, volume);
        }
        else
        {
            efxSource6.volume = volume * GlobalController.SoundVolume;
            efxSource6.clip = clip;
            efxSource6.loop = loop;
            efxSource6.pitch = pitch;
            efxSource6.Play();
            return efxSource6;
        }
    }

    private AudioSource PlaySingle6(AudioClip clip, float pitch = 1f, bool loop = false, float volume = 1)
    {
        if (efxSource7.isPlaying)
        {
            return PlaySingle7(clip, pitch, loop, volume);
        }
        else
        {
            efxSource7.volume = volume * GlobalController.SoundVolume;
            efxSource7.clip = clip;
            efxSource7.loop = loop;
            efxSource7.pitch = pitch;
            efxSource7.Play();
            return efxSource7;
        }
    }

    private AudioSource PlaySingle7(AudioClip clip, float pitch = 1f, bool loop = false, float volume = 1)
    {
        if (efxSource8.isPlaying)
        {
            return PlaySingle8(clip, pitch, loop, volume);
        }
        else
        {
            efxSource8.volume = volume * GlobalController.SoundVolume;
            efxSource8.clip = clip;
            efxSource8.loop = loop;
            efxSource8.pitch = pitch;
            efxSource8.Play();
            return efxSource8;
        }
    }

    private AudioSource PlaySingle8(AudioClip clip, float pitch = 1f, bool loop = false, float volume = 1)
    {
        efxSource9.volume = volume * GlobalController.SoundVolume;
        efxSource9.clip = clip;
        efxSource9.loop = loop;
        efxSource9.pitch = pitch;
        efxSource9.Play();
        return efxSource9;
    }

    public void StopLoopSound(AudioClip clip)
    {
        if (efxSource1 == null) return;
        if (efxSource1.clip == clip)
        {
            efxSource1.loop = false;
        }
        else if (efxSource2.clip == clip)
        {
            efxSource2.loop = false;
        }
        else if (efxSource3.clip == clip)
        {
            efxSource3.loop = false;
        }
        else if (efxSource4.clip == clip)
        {
            efxSource4.loop = false;
        }
        else if (efxSource5.clip == clip)
        {
            efxSource5.loop = false;
        }
        else if (efxSource6.clip == clip)
        {
            efxSource6.loop = false;
        }
        else if (efxSource7.clip == clip)
        {
            efxSource7.loop = false;
        }
        else if (efxSource8.clip == clip)
        {
            efxSource8.loop = false;
        }
        else if (efxSource9.clip == clip)
        {
            efxSource9.loop = false;
        }
    }

    public AudioSource PlaySandSound(AudioClip clip, float pitch = 1f, bool loop = false, float volume = 1f)
    {
        if (!GlobalController.IsSoundOn || clip == null || sandSource == null) return null;

        if (loop)
        {
            if (sandSource.isPlaying && sandSource.clip == clip && sandSource.loop) return sandSource;
            sandSource.clip = clip;
            sandSource.volume = volume * GlobalController.SoundVolume;
            sandSource.pitch = pitch;
            sandSource.loop = true;
            sandSource.Play();
        }
        else
        {
            sandSource.pitch = pitch;
            sandSource.PlayOneShot(clip, volume * GlobalController.SoundVolume);
        }

        return sandSource;
    }

    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        if (GlobalController.IsSoundOn)
        {
            musicSource.clip = clip;
            musicSource.loop = loop;
            musicSource.Play();
        }
        else
        {
            musicSource.volume = 0;
            musicSource.clip = clip;
            musicSource.loop = loop;
            musicSource.Play();
        }
    }

    public void StopMusic()
    {
        StoppedMusic = musicSource.clip;
        musicSource.Stop();
    }

    public void ReplayMusic()
    {
        if (StoppedMusic != null)
        {
            musicSource.clip = StoppedMusic;
            musicSource.Play();
        }
    }

    public void Mute()
    {
        musicSource.volume = 0;
    }

    public void Unmute()
    {
        //musicSource.volume = 1;
        if (musicSource != null) musicSource.volume = GlobalController.BgmVolume;
    }

    public void SetBgmVolume(float volume)
    {
        if (musicSource != null)
        {
            musicSource.volume = volume;
        }
    }

    public void SetSoundVolume(float volume)
    {
        // Gom tất cả các AudioSource phát SFX lại để cập nhật âm lượng cùng lúc
        AudioSource[] allSfxSources = new AudioSource[]
        {
            efxSource1, efxSource2, efxSource3, efxSource4, efxSource5,
            efxSource6, efxSource7, efxSource8, efxSource9,
            ExclusiveSource, ExclusiveSource1, ExclusiveSource2, ExclusiveSource3, ExclusiveSource4
        };

        foreach (var source in allSfxSources)
        {
            if (source != null)
            {
                source.volume = volume;
            }
        }
    }

    /// <summary>
    /// Fade music volume
    /// </summary>
    /// <param name="to"></param>
    /// <param name="duration"></param>
    /// <param name="revertOrigin">True if musicSource.volume should be reverted to start value</param>
    /// <param name="from"></param>
    public void FadeMusic(float to, float duration, bool revertOrigin = true, float from = -1)
    {
        if (from >= 0)
        {
            musicSource.volume = from;
        }
        float startVolume = musicSource.volume;
        LeanTween.value(musicSource.volume, to, duration).setOnUpdate((float f) =>
        {
            musicSource.volume = f;
        }).setOnComplete(() =>
        {
            if (to == 0)
            {
                StopMusic();
            }
            if (revertOrigin)
            {
                musicSource.volume = startVolume;
            }
        });
    }

    public void StopAllSounds()
    {
        efxSource1.Stop();
        efxSource2.Stop();
        efxSource3.Stop();
        efxSource4.Stop();
        efxSource5.Stop();
        efxSource6.Stop();
        efxSource7.Stop();
        efxSource8.Stop();
        efxSource9.Stop();
    }

    public void PauseSounds()
    {
        efxSource1.Pause();
        efxSource2.Pause();
        efxSource3.Pause();
        efxSource4.Pause();
        efxSource5.Pause();
        efxSource6.Pause();
        efxSource7.Pause();
        efxSource8.Pause();
        efxSource9.Pause();
    }

    public void UnpauseSounds()
    {
        efxSource1.UnPause();
        efxSource2.UnPause();
        efxSource3.UnPause();
        efxSource4.UnPause();
        efxSource5.UnPause();
        efxSource6.UnPause();
        efxSource7.UnPause();
        efxSource8.UnPause();
        efxSource9.UnPause();
    }
}