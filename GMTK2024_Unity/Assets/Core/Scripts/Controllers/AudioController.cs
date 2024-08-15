using UnityEngine;
using UnityEngine.Audio;

public class AudioController : SingletonMonobehaviour<AudioController>
{
	public const int MinValue = 0;
	public const int MaxValue = 9;
	private const float MinVolume = -80f;
	private const float MaxVolume = 0f;
	private const string MusicVolumeParameterName = "MusicVolume";
	private const string SoundVolumelumeParameterName = "SoundVolume";

	[SerializeField] private AudioMixer audioMixer;

	public int MusicValue
	{
		get
		{
			return audioMixer.GetFloat(MusicVolumeParameterName, out float volume) ? VolumeToValue(volume) : 0;
		}
		set
		{
			audioMixer.SetFloat(MusicVolumeParameterName, ValueToVolume(value));
			PlayerPrefs.SetInt(MusicVolumeParameterName, value);
		}
	}

	public int SoundValue
	{
		get
		{
			return audioMixer.GetFloat(SoundVolumelumeParameterName, out float volume) ? VolumeToValue(volume) : 0;
		}
		set
		{
			audioMixer.SetFloat(SoundVolumelumeParameterName, ValueToVolume(value));
			PlayerPrefs.SetInt(SoundVolumelumeParameterName, value);
		}
	}

	// ======== Unity Messages ========

	private void Start()
	{
		SetupSavedInfo();
		SetVolumeFromSavedInfo();

	}

	// ======== Functionalities ========

	private void SetupSavedInfo()
	{
		if (!PlayerPrefs.HasKey(MusicVolumeParameterName))
		{
			PlayerPrefs.SetInt(MusicVolumeParameterName, MaxValue);
			PlayerPrefs.Save();
		}
		if (!PlayerPrefs.HasKey(SoundVolumelumeParameterName))
		{
			PlayerPrefs.SetInt(SoundVolumelumeParameterName, MaxValue);
			PlayerPrefs.Save();
		}
	}

	private void SetVolumeFromSavedInfo()
	{
		MusicValue = PlayerPrefs.GetInt(MusicVolumeParameterName);
		SoundValue = PlayerPrefs.GetInt(SoundVolumelumeParameterName);
	}

	private float ValueToVolume(int value)
	{
		return Mathf.Lerp(MinVolume, MaxVolume, Mathf.InverseLerp(MinValue, MaxValue, value));
	}

	private int VolumeToValue(float volume)
	{
		return Mathf.RoundToInt(Mathf.Lerp(MinValue, MaxValue, Mathf.InverseLerp(MinVolume, MaxVolume, volume)));
	}
}
