using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndingController : MonoBehaviour
{
	[Header("References")]
	[SerializeField] private new Camera camera;

	private bool _transitioning;

	// ======== Unity Messages ========

	private void Awake()
	{
		Application.targetFrameRate = 60;
	}

	private void Start()
	{
		OverlayController.Fade(Color.clear);
	}
	private void Update()
	{
		CameraMovement();
	}

	// ======== Ending Buttons ========

	public void BackToMainMenu()
	{
		if (_transitioning)
			return;
		_transitioning = true;

		StartCoroutine(TransitionSceneSequence());
		IEnumerator TransitionSceneSequence()
		{
			OverlayController.Fade(Color.black);
			yield return new WaitForSeconds(0.5f);
			SceneManager.LoadScene(0);
		}
	}

	// ======== Camera ========

	private void CameraMovement()
	{
		camera.transform.localEulerAngles = new Vector3(0f, Mathf.Cos(Time.time / 4f) * 1f, 0f);
	}
}
