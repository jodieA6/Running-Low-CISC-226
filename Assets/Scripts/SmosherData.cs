using UnityEngine;
[System.Serializable]
public class SmosherData
{
	public string smosherName;
	public string smosherTriggerName;
	public string stopColliderName;

	public Vector3 topPosition;
	public Vector3 bottomPosition;

	public float dropSpeed;
	public float riseSpeed;
	public float waitAtTop;
	public float waitAtBottom;
	public bool playThudOnLand;
}