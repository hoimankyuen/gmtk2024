using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
	public static GameController Instance;

	public bool IsStarted { get; private set; }
	public bool IsPaused { get; private set; }
	public bool IsEnded { get; private set; }

	public event Action OnLevelReadyToStartEvent;

	public event Action OnLevelStartedEvent;
	public event Action OnLevelPausedEvent;
	public event Action OnLevelResumedEvent;
	public event Action OnLevelEndedEvent;

	// ======== Unity Messages ========

	private void Awake()
	{
		Instance = this;

		Application.targetFrameRate = 60;
		Cursor.lockState = CursorLockMode.Locked;
	}

	private void Start()
	{
		DelayedStartTrigger();
	}

	private void Update()
	{
		TogglePause();
	}

	private void OnDestroy()
	{
		Instance = null;
	}

	// ======== Start Level ========

	private void DelayedStartTrigger()
	{
		StartCoroutine(Sequence());
		IEnumerator Sequence()
		{
			yield return null;
			OverlayController.Fade(Color.clear);
			OnLevelReadyToStartEvent?.Invoke();
		}
	}

	public void StartLevel()
	{
		if (IsStarted)
			return;

		IsStarted = true;
		OnLevelStartedEvent?.Invoke();
	}

	// ======== Pause Level ========

	private void TogglePause()
	{
		if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
		{
			PauseLevel(!IsPaused);
		}
	}
	public void PauseLevel(bool paused)
	{
		if (!IsStarted)
			return;
		if (IsEnded)
			return;

		if (paused)
		{
			Cursor.lockState = CursorLockMode.None;
			IsPaused = true;
			OnLevelPausedEvent?.Invoke();
		}
		else
		{
			Cursor.lockState = CursorLockMode.Locked;
			IsPaused = false;
			OnLevelResumedEvent?.Invoke();
		}
	}

	// ======== End Level ========

	public void EndLevel()
	{
		if (!IsStarted)
			return;
		if (IsPaused)
			return;

		IsEnded = true;
		OnLevelEndedEvent?.Invoke();
		Cursor.lockState = CursorLockMode.None;

		PlayerPrefs.SetInt("UnlockedLevel", SceneManager.GetActiveScene().buildIndex + 1);
		PlayerPrefs.Save();

		OverlayController.Fade(Color.black);

		StartCoroutine(TransitionSceneSequence());
		IEnumerator TransitionSceneSequence()
		{
			yield return new WaitForSeconds(0.5f);
			int targetSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
			if (targetSceneIndex > SceneManager.sceneCountInBuildSettings - 1)
			{
				targetSceneIndex = 0;
			}
			SceneManager.LoadScene(targetSceneIndex);
		}
	}

	public void BackToMainMenu()
	{
		IsEnded = true;
		OnLevelEndedEvent?.Invoke();
		Cursor.lockState = CursorLockMode.None;

		StartCoroutine(TransitionSceneSequence());
		IEnumerator TransitionSceneSequence()
		{
			OverlayController.Fade(Color.black);
			yield return new WaitForSeconds(0.5f);
			SceneManager.LoadScene(0);
		}
	}
}