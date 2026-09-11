using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class AudioManager : MonoBehaviour
{
    public Sound[] sounds;
    public static AudioManager audioManager;

   private const string BgmSoundName = "bg";

   [Header("BGM Ducking")]
   [Tooltip("BGM volume is multiplied by this while an activity/game is open, then restored to normal when you return to page selection.")]
   [Range(0f, 1f)]
   [SerializeField] private float bgmDuckMultiplier = 0.3f;

   [Header("Activity Audio")]
   [Tooltip("Volume used for the clip assigned per activity in LearningContentData > Activities > Activity Audio.")]
   [Range(0f, 1f)]
   [SerializeField] private float activityAudioVolume = 1f;

   // Dedicated source for the per-activity clip. It's kept out of `sounds`
   // because that list is keyed by name and shared/looked up globally, while
   // this one changes clip every time a different activity is opened.
   private AudioSource activityAudioSource;

   private void Awake()
   {
      if (audioManager == null)
      {
         audioManager = this;
      }
      else
      {
         Destroy(gameObject);
         return;
      }

      foreach (Sound s in sounds)
      {
         s.audioSource = gameObject.AddComponent<AudioSource>();
         s.audioSource.clip = s.clip;
         s.audioSource.volume = s.volume;
         s.audioSource.loop = s.loop;
      }

      activityAudioSource = gameObject.AddComponent<AudioSource>();
      activityAudioSource.playOnAwake = false;
      activityAudioSource.loop = false;

      DontDestroyOnLoad(gameObject);
   }

   // BGM is no longer auto-started here - IntroVideoController starts it
   // once the intro video is done (or immediately if there's no video to
   // show), so it never plays underneath the video.

   /// <summary>
   /// Lowers the BGM volume (e.g. while an activity/game is open) without
   /// affecting its assigned base volume, so RestoreBgmVolume() can bring it
   /// back to exactly where it started.
   /// </summary>
   public void DuckBgmVolume()
   {
      Sound s = Array.Find(sounds, sound => sound.name == BgmSoundName);

      if (s == null || s.audioSource == null)
         return;

      s.audioSource.volume = s.volume * bgmDuckMultiplier;
   }

   /// <summary>
   /// Restores the BGM to its normal (assigned) volume - call this when
   /// returning to the page-selection screen.
   /// </summary>
   public void RestoreBgmVolume()
   {
      Sound s = Array.Find(sounds, sound => sound.name == BgmSoundName);

      if (s == null || s.audioSource == null)
         return;

      s.audioSource.volume = s.volume;
   }

   /// <summary>
   /// Plays the clip assigned to an activity (LearningContentData >
   /// ActivityPageData > Activity Audio). These clips aren't in the named
   /// `sounds` list, so they get their own source. Whatever that source was
   /// already playing is stopped first - including when <paramref name="clip"/>
   /// is null - so opening an activity never leaves the previous activity's
   /// audio playing underneath it.
   /// </summary>
   public void PlayActivityAudio(AudioClip clip)
   {
      if (activityAudioSource == null)
         return;

      activityAudioSource.Stop();
      activityAudioSource.clip = clip;

      if (clip == null)
         return;

      activityAudioSource.volume = activityAudioVolume;
      activityAudioSource.Play();
   }

   /// <summary>
   /// Stops the current activity's audio - call this when the activity is
   /// closed so it doesn't carry over to the page-selection screen.
   /// </summary>
   public void StopActivityAudio()
   {
      PlayActivityAudio(null);
   }

   public bool IsPlaying(string name)
   {
      Sound s = Array.Find(sounds, sound => sound.name == name);

      if (s == null)
         return false;

      return s.audioSource.isPlaying;
   }

   public void Play(string name)
   {
      Sound s = Array.Find(sounds, sound => sound.name == name);

      if (s == null)
      {
         Debug.LogWarning("Sound not found : " + name);
         return;
      }

      // Background music or any looping sound
      if (s.loop)
      {
         if (!s.audioSource.isPlaying)
         {
            s.audioSource.clip = s.clip;
            s.audioSource.volume = s.volume;
            s.audioSource.loop = true;
            s.audioSource.Play();
         }
      }
      // Sound Effects
      else
      {
         s.audioSource.PlayOneShot(s.clip, s.volume);
      }
   }

   public void Pause(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);

        if (s.name == null)
        {
            return;
        }
        s.audioSource.Pause();
    }
    
    public void StopSound()
    {
       foreach(Sound s in sounds)
       {
            if (s.name != "bg")
                s.audioSource.Stop();
        }

        StopActivityAudio();
    }

    public void PauseSound()
    {
        foreach (Sound s in sounds)
        {
            if (s.audioSource.isPlaying)
                s.audioSource.Pause();
        }

        if (activityAudioSource != null && activityAudioSource.isPlaying)
            activityAudioSource.Pause();
    }

    public void PlayPausedSound()
    {
        foreach (Sound s in sounds)
        {

            s.audioSource.UnPause();
        }

        if (activityAudioSource != null)
            activityAudioSource.UnPause();
    }


    private void OnApplicationFocus(bool focus)
    {
        if (focus)
        {
            PlayPausedSound();
        }
        else
        {
            PauseSound();
        }
    }
}

   

