using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
public class B3b0TextController : MonoBehaviour
{
	[SerializeField] TMP_Text textObject;
	private string FinalText;
	private string CurrentText = "";
	private int TextLength = 0;
	private int currentTextLength = 0;
	float timer = 0f;
	public float interval = 0.05f; // seconds per character
	private bool StopsTime = false;
	private bool IsTutorialText = false;
	public AudioSource audioSource;
	public AudioClip audioClip;
	[SerializeField] public GameObject RayCastPanel;
	private PlayerController playerController;

	// Start is called once before the first execution of Update after the MonoBehaviour is created
	void Start()
	{
		if (audioSource == null)
		{
			audioSource = GetComponent<AudioSource>();
		}
	}
	// Update is called once per frame
	void Update()
	{
		if (StopsTime)
		{
			if (Mouse.current.leftButton.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame)
			{
				if (IsComplete())
				{
					if (!IsTutorialText)
					{
						Time.timeScale = 1f;
						if (playerController != null)
							playerController.acceptingInput = true;
					}
					Destroy(gameObject);
				}
				else
				{
					JumpToEnd();
				}
			}
		}

		if (FinalText == null || currentTextLength >= FinalText.Length)
		{
			return;
		}
		timer += Time.unscaledDeltaTime;
		if (timer >= interval)
		{
			timer = 0f;
			currentTextLength++;
			CurrentText = FinalText[..currentTextLength];
			textObject.text = CurrentText;
			if (currentTextLength % 2 == 0 && CurrentText[currentTextLength - 1] != ' ')
			{
				//Play Audio
				if (audioSource != null)
				{
					PlayBeep();
				}
			}
		}

	}
	private float minPitch = .9f;
	private float maxPitch = 1.2f;
	public void PlayBeep()
	{
		GameObject temp = new GameObject("TempAudio");
		AudioSource tempSource = temp.AddComponent<AudioSource>();
		tempSource.clip = audioClip;
		tempSource.pitch = Random.Range(minPitch, maxPitch);
		tempSource.Play();
		Destroy(temp, audioClip.length / tempSource.pitch);
	}
	public void Initiate(string target, bool StopsTime = false, bool UIText = false)
	{
		FinalText = target;
		TextLength = target.Length;
		if (TextLength == 0)
		{
			textObject.text = "[ERROR] Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. ";
		}
		if (StopsTime)
		{
			playerController = FindFirstObjectByType<PlayerController>();
			if (playerController != null)
				playerController.acceptingInput = false;
			IsTutorialText = UIText;
			Time.timeScale = 0f;
			this.StopsTime = true;
		}
		if (!UIText)
		{
			Destroy(RayCastPanel);
		}
	}
	public bool IsComplete()
	{
		return (currentTextLength == TextLength);

	}
	public void JumpToEnd()
	{
		CurrentText = FinalText;
		textObject.text = CurrentText;
		currentTextLength = TextLength;
	}
}