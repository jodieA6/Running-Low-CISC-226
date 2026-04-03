using UnityEngine;
using System.Collections;

public class BoxLauncher : MonoBehaviour
{
	public Vector2 launchDirection = Vector2.up;
	public float launchForce = 10f;

	[Header("Flash")]
	[SerializeField] private Color flashColor = Color.red;
	[SerializeField] private float flashDuration = 0.5f;

	private SpriteRenderer sr;
	private Coroutine flashCoroutine;
	private bool isFlashing = false;

	void Awake()
	{
		sr = GetComponentInChildren<SpriteRenderer>();
		Debug.Log("BoxLauncher SR found: " + sr);
	}

	public void Flash()
	{
		if (sr == null || isFlashing) return;
		flashCoroutine = StartCoroutine(DoFlash());
	}

	private IEnumerator DoFlash()
	{
		isFlashing = true;
		Color original = sr.color;
		sr.color = flashColor;
		yield return new WaitForSeconds(flashDuration);
		sr.color = original;
		isFlashing = false;
		flashCoroutine = null;
	}
}