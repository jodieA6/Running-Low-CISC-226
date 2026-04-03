using UnityEngine;

public class EndZone : MonoBehaviour
{
	private GameManager gameManager;

	void Start()
	{
		gameManager = FindFirstObjectByType<GameManager>();
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (other.CompareTag("Player"))
		{
			gameManager.OnEndZoneReached();
			STATIC_DATA.Tutorial = false;
		}
	}
}
