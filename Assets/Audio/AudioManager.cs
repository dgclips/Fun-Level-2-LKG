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
    }

    public void PauseSound()
    {
        foreach (Sound s in sounds)
        {
            if (s.audioSource.isPlaying)
                s.audioSource.Pause();
        }
    }

    public void PlayPausedSound()
    {
        foreach (Sound s in sounds)
        {

            s.audioSource.UnPause();
        }
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

   

