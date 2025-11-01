using System.Collections;
using System.Collections.Generic;
using tuleeeeee.Misc;
using tuleeeeee.Utilities;
using UnityEngine;


[DisallowMultipleComponent]
public class SoundEffectManager : SingletonMonoBehaviour<SoundEffectManager>
{
    public int soundsVolume = 5;

    private void Start()
    {
        if (PlayerPrefs.HasKey(Settings.SoundsVolumeKey))
        {
            soundsVolume = PlayerPrefs.GetInt(Settings.SoundsVolumeKey);
        }

        SetSoundsVolume(soundsVolume);
    }

    private void OnDisable()
    {
        PlayerPrefs.SetInt(Settings.SoundsVolumeKey, soundsVolume);
    }

    public void PlaySoundEffect(SoundEffectSO soundEffect)
    {
        if (soundEffect == null || PoolManager.Instance == null) return;

        SoundEffect sound = (SoundEffect)PoolManager.Instance.ReuseComponent(soundEffect.soundPrefab, Vector3.zero, Quaternion.identity);
        if (sound == null)
        {
            Debug.LogError("Failed to retrieve SoundEffect from PoolManager.");
            return;
        }
        sound.SetSound(soundEffect);
        sound.gameObject.SetActive(true);
        StartCoroutine(DisableSound(sound, soundEffect.soundEffectClip.length));
    }

    private IEnumerator DisableSound(SoundEffect sound, float soundDuration)
    {
        yield return new WaitForSeconds(soundDuration);
        sound.gameObject.SetActive(false);
    }

    public void IncreaseSoundVolume()
    {
        int maxSoundVolume = 20;

        if (soundsVolume >= maxSoundVolume) return;

        soundsVolume += 1;

        SetSoundsVolume(soundsVolume);
    }

    public void DecreaseSoundVolume()
    {
        int minSoundVolume = 0;

        if (soundsVolume <= minSoundVolume) return;

        soundsVolume -= 1;

        SetSoundsVolume(soundsVolume);
    }

    private void SetSoundsVolume(int soundsVolume)
    {
        float muteDecibels = -80f;
        if (soundsVolume == 0)
        {
            GameResources.Instance.soundsMasterMixerGroup.audioMixer.SetFloat(Settings.SoundsVolumeKey,
                muteDecibels);
        }
        else
        {
            GameResources.Instance.soundsMasterMixerGroup.audioMixer.SetFloat(Settings.SoundsVolumeKey, 
                HelperUtilities.LinearToDecibels(soundsVolume));
        }
    }
}
