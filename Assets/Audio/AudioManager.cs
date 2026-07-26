using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class AudioManager : MonoBehaviour
{
    public Sound[] sounds;
    public static AudioManager audioManager;
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

   // Start is called before the first frame update
   void Start()
    {
       // Play("bg");
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

   

