using UnityEngine;
[CreateAssetMenu(fileName = "LevelData", menuName = "Game/Level Data")]
public class Leveldata : ScriptableObject
{
	[Header("Scene")]
	public string nextLevel;

	[Header("Player")]
	public Vector3[] spawnLocations;
	public Vector3[] cameraPositions;

	[Header("Transitions")]
	public string[] screenTransitions;

	[Header("Waterfalls")]
	public Collider2D waterfalls;

	[Header("Smoshers")]
	public SmosherData[] smoshers;
	public float smosherLaunchForce;

}