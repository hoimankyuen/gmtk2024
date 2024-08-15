using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPauseMenu : MonoBehaviour
{
	[Header("References")]
	[SerializeField] private GameController gameController;

	[Header("Component")]
	[SerializeField] private GameObject menuBody;

	// ======== Unity Messages ========

	private void Start()
	{
		gameController.OnLevelPausedEvent += OnLevelPaused;
		gameController.OnLevelResumedEvent += OnLevelResumed;
	}

	private void OnDestroy()
	{
		if (gameController != null)
		{
			gameController.OnLevelPausedEvent -= OnLevelPaused;
			gameController.OnLevelResumedEvent -= OnLevelResumed;
		}
	}

	// ======== Game Manager Messages ========

	private void OnLevelPaused()
	{
		menuBody.SetActive(gameController.IsPaused);
	}

	private void OnLevelResumed()
	{
		menuBody.SetActive(gameController.IsPaused);
	}

	// ======== Buttons ========

	public void ResumeGame()
	{
		gameController.PauseLevel(false);
	}

	public void BackToMainMenu()
	{
		gameController.BackToMainMenu();
	}
}
