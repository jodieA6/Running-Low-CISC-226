using UnityEngine;

public class SpriteFlip : MonoBehaviour
{
	private SpriteRenderer sr;
	private Transform parent;

	void Start()
	{
		sr = GetComponent<SpriteRenderer>();
		parent = transform.root;
	}

	void Update()
	{
		Debug.Log("Child Y rotation after fix: " + transform.eulerAngles.y);
		sr = GetComponent<SpriteRenderer>();
		parent = transform.parent;

		if (parent.eulerAngles.y > 90f)
		{
			sr.flipX = true;
			transform.eulerAngles = new Vector3(transform.eulerAngles.x, 0, transform.eulerAngles.z);
		}
		else
		{
			sr.flipX = false;
			transform.eulerAngles = new Vector3(transform.eulerAngles.x, 0, transform.eulerAngles.z);
		}
	}
}
