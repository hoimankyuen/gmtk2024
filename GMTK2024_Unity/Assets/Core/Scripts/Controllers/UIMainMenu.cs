using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIMainMenu : MonoBehaviour
{
	public enum Page
	{
		Main,
		Chapters,
		Controls,
		Settings,
		Credit
	}

	[Header("Page Conponents")]
	[SerializeField] private CanvasGroup mainCanvasGroup;
	[SerializeField] private CanvasGroup chaptersCanvasGroup;
	[SerializeField] private CanvasGroup controlsCanvasGroup;
	[SerializeField] private CanvasGroup settingsCanvasGroup;
	[SerializeField] private CanvasGroup creditsCanvasGroup;

	[Header("Chapters Components")]
	[SerializeField] private Button chapter2Button;

	[Header("Settings Components")]
	[SerializeField] private UIStepFillBar musicStepFillBar;
	[SerializeField] private UIStepFillBar soundStepFillBar;

	private AudioController _audioController;

	private bool _transitioning;

	// ======== Unity Messages ========

	private void Awake()
	{
		Application.targetFrameRate = 60;

		if (!PlayerPrefs.HasKey("UnlockedLevel"))
		{
			PlayerPrefs.SetInt("UnlockedLevel", 1);
			PlayerPrefs.Save();
		}

		RefreshChaptersPageComponents();
	}

	private void Start()
	{
		_audioController = AudioController.Instance;

		OverlayController.Fade(Color.clear);
	}

	// ======== Main Page ========
	
	public void ShowChapters()
	{
		ShowPage(Page.Chapters);
		RefreshChaptersPageComponents();
	}

	public void ShowControls()
	{
		ShowPage(Page.Controls);
	}

	public void ShowSettings()
	{
		ShowPage(Page.Settings);
		RefreshSettingsPageComponents();
	}

	public void ShowCredits()
	{
		ShowPage(Page.Credit);
	}

	public void ShowMain()
	{
		ShowPage(Page.Main);
	}

	private void ShowPage(Page page)
	{
		StartCoroutine(ShowContainer(mainCanvasGroup, page == Page.Main));
		//StartCoroutine(ShowContainer(chaptersCanvasGroup, page == Page.Chapters));
		StartCoroutine(ShowContainer(controlsCanvasGroup, page == Page.Controls));
		StartCoroutine(ShowContainer(settingsCanvasGroup, page == Page.Settings));
		StartCoroutine(ShowContainer(creditsCanvasGroup, page == Page.Credit));
	}

	private IEnumerator ShowContainer(CanvasGroup canvasGroup, bool show)
	{
		float elapsed = 0;
		float duration = 0.2f;
		if (show)
		{
			yield return new WaitForSeconds(duration);
			while (elapsed < duration)
			{
				canvasGroup.alpha = elapsed / duration;
				elapsed += Time.deltaTime;
				yield return null;
			}
			canvasGroup.interactable = true;
			canvasGroup.blocksRaycasts = true;
			canvasGroup.alpha = 1;
		}
		else if (canvasGroup.alpha == 1)
		{
			canvasGroup.interactable = false;
			canvasGroup.blocksRaycasts = false;
			while (elapsed < duration)
			{
				canvasGroup.alpha = 1f - elapsed / duration;
				elapsed += Time.deltaTime;
				yield return null;
			}
			canvasGroup.alpha = 0;
			yield return new WaitForSeconds(duration);
		}
	}

	public void ExitGame()
	{
		Application.Quit();
	}

	// ======== Chapters Page ========

	private void RefreshChaptersPageComponents()
	{
		chapter2Button.interactable = PlayerPrefs.GetInt("UnlockedLevel") <= 1;
		chapter2Button.interactable = PlayerPrefs.GetInt("UnlockedLevel") <= 2;
	}

	public void StartChapter(int sceneIndex)
	{
		if (_transitioning)
			return;
		_transitioning = true;

		StartCoroutine(TransitionSceneSequence());
		IEnumerator TransitionSceneSequence()
		{
			OverlayController.Fade(Color.black);
			yield return new WaitForSeconds(0.5f);
			SceneManager.LoadScene(sceneIndex);
		}
	}

	// ======== Settings Page ========
	
	private void RefreshSettingsPageComponents()
	{
		musicStepFillBar.MinValue = AudioController.MinValue;
		musicStepFillBar.MaxValue = AudioController.MaxValue;
		musicStepFillBar.Value =_audioController.MusicValue;

		soundStepFillBar.MinValue = AudioController.MinValue;
		soundStepFillBar.MaxValue = AudioController.MaxValue;
		soundStepFillBar.Value = _audioController.SoundValue;
	}

	public void SaveSettings()
	{
		_audioController.MusicValue = musicStepFillBar.Value;
		_audioController.SoundValue = soundStepFillBar.Value;
	}
}
