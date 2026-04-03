using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour
{
	[SerializeField] private Leveldata level;
	[SerializeField] private GameObject PauseMenu;
	[SerializeField] private FadeOutController FadeController;
	[SerializeField] private SaveFileManager saveFileManager;
	[SerializeField] private AudioSource thudSound;
	[SerializeField] private GameObject respawnParticles;

	private bool isTransitioning;
	private int currentScreen = 0;

	private GameObject playerObject;
	private Collider2D playerCollider;
	private SpriteRenderer playerRenderer;
	private Rigidbody2D playerRigidbody;

	// Level object references
	private List<SmosherRuntime> smosherRefs = new();
	private CompositeCollider2D water;
	private Collider2D[] transitions;
	private Collider2D[] killZones;
	private Collider2D[] waterfalls;

	// Active coroutines
	private List<Coroutine> activeCoroutines = new();

	private PlayerModifiers playerModifiers;
	private TimerScript timer;
	private PlayerController playerController;

	public bool paused = false;

	// global beat timer, reset on LoadLevel so all smoshers start at t=0
	private float globalBeatTimer = 0f;

	// which smosher the player is currently riding
	private SmosherRuntime riddenSmosher = null;

	// how long to disable PlayerController after a launch so groundedDrag
	private const float LaunchLockoutDuration = 0.1f;
	private float launchLockoutTimer = 0f;

	//launching boxes
	private BoxLauncher[] launchBoxLaunchers;
	private Rigidbody2D[] launchBoxBodies;
	private HashSet<Rigidbody2D> launchedBoxes = new();
	private Vector3[] launchBoxStartPositions;
	private Dictionary<Rigidbody2D, BoxLauncher> boxToLauncher = new();

	private Collider2D[] gasZones;

	// smosher thud tracking
	private Dictionary<SmosherRuntime, bool> smosherWasMoving = new();
	private const float SmosherStopThreshold = 0.05f;
	private const float SmosherBottomTolerance = 1f;

	[SerializeField] private float fallKillY = -20f;

	private class SmosherRuntime
	{
		public SmosherData data;
		public GameObject smosherObject;
		public GameObject triggerObject;
		public Collider2D[] smosherColliders;
		public Collider2D triggerCollider;
		public Collider2D stopCollider;
	}

	void Start()
	{
		STATIC_DATA.CURRENT_LEVEL = SceneManager.GetActiveScene().name;
		saveFileManager.SaveData();
		timer = FindFirstObjectByType<TimerScript>();
		Debug.Log("timer" + timer);
		playerModifiers = FindFirstObjectByType<PlayerModifiers>();
		Debug.Log("Playermodifier" + playerModifiers);
		playerObject = GameObject.Find("Player");
		Debug.Log("Player" + playerObject);
		playerController = playerObject.GetComponent<PlayerController>();
		Debug.Log("PalyerController" + playerController);
		playerCollider = playerObject.GetComponent<Collider2D>();
		Debug.Log("PlayerCollider" + playerCollider);
		playerRenderer = playerObject.transform.Find("PlayerSprite").GetComponent<SpriteRenderer>();
		Debug.Log("PlayerRenderer" + playerRenderer);
		playerRigidbody = playerObject.GetComponent<Rigidbody2D>();
		Debug.Log("RigidBody" + playerRigidbody);
		water = GameObject.Find("Water").GetComponent<CompositeCollider2D>();
		Debug.Log("Water" + water);
		LoadLevel();
		isTransitioning = false;
		if (!STATIC_DATA.Tutorial)
		{
			playerController.doubleJumpCounter = 2;
		}
		Debug.Log("Tutorial: " + STATIC_DATA.Tutorial.ToString());
	}

	private void Update()
	{
		globalBeatTimer += Time.deltaTime;

		if (launchLockoutTimer > 0f)
		{
			launchLockoutTimer -= Time.deltaTime;
			if (launchLockoutTimer <= 0f)
				playerController.enabled = true;
		}
		HandleHazards();
		HandleRiding();
		HandleTransitions();
		CheckPause();
		HandleLaunchBoxes();
		HandleSmosherThuds();
	}

	// -------------------------------------------------------------------------
	// Pause
	// -------------------------------------------------------------------------

	public void TogglePause()
	{
		paused = !paused;
		Time.timeScale = paused ? 0f : 1f;
		PauseMenu.SetActive(paused);
	}

	private void CheckPause()
	{
		if (Keyboard.current.escapeKey.wasPressedThisFrame)
			TogglePause();
	}

	// -------------------------------------------------------------------------
	// Riding detection & boost
	// -------------------------------------------------------------------------

	private const float RaycastSkinWidth = 0.1f;

	private void HandleRiding()
	{
		SmosherRuntime newRidden = null;

		Vector2 rayOrigin = new Vector2(
			playerCollider.bounds.center.x,
			playerCollider.bounds.min.y
		);

		Debug.DrawRay(rayOrigin, Vector2.down * RaycastSkinWidth, Color.red);

		foreach (SmosherRuntime smosher in smosherRefs)
		{
			if (smosher.smosherColliders == null || smosher.smosherColliders.Length == 0) continue;

			Vector2 checkPoint = new Vector2(rayOrigin.x, rayOrigin.y - RaycastSkinWidth);

			foreach (Collider2D col in smosher.smosherColliders)
			{
				if (col == null) continue;
				bool overlapHit = col.OverlapPoint(checkPoint);
				bool touching = playerCollider.IsTouching(col);
				bool feetAbove = playerCollider.bounds.min.y >= col.bounds.max.y - 0.1f;

				if (overlapHit || (touching && feetAbove))
				{
					newRidden = smosher;
					Debug.Log($"Hit smosher: {smosher.data.smosherName} via col={col.name}");
					break;
				}
			}

			if (newRidden != null) break;
		}

		if (riddenSmosher != null && riddenSmosher != newRidden)
			DetachFromSmosher();

		if (newRidden != null)
		{
			if (riddenSmosher == null)
			{
				Debug.Log($"Attaching to smosher: {newRidden.data.smosherName}");
				playerObject.transform.SetParent(newRidden.smosherObject.transform);
			}
			riddenSmosher = newRidden;
		}
	}

	private void DetachFromSmosher()
	{
		playerObject.transform.SetParent(null);
		riddenSmosher = null;
		launchLockoutTimer = 0f;
		if (playerController != null)
			playerController.enabled = true;
	}

	private void ApplyLaunchBoost()
	{
		if (playerRigidbody == null) return;

		playerController.enabled = false;
		launchLockoutTimer = LaunchLockoutDuration;

		Vector2 vel = playerRigidbody.linearVelocity;
		vel.y = 0f;
		playerRigidbody.linearVelocity = vel;
		playerRigidbody.AddForce(Vector2.up * level.smosherLaunchForce, ForceMode2D.Impulse);
	}

	// -------------------------------------------------------------------------
	// Hazard detection
	// -------------------------------------------------------------------------

	private void HandleHazards()
	{
		if (playerObject.transform.position.y < fallKillY)
		{
			DetachFromSmosher();
			RespawnPlayer();
			return;
		}

		if (killZones == null || waterfalls == null || gasZones == null) return;

		foreach (SmosherRuntime smosher in smosherRefs)
		{
			if (smosher.triggerCollider != null && playerCollider.IsTouching(smosher.triggerCollider))
			{
				DetachFromSmosher();
				RespawnPlayer();
				return;
			}
		}

		if (water != null && playerCollider.IsTouching(water) && playerModifiers.GetWaterProof() == false)
		{
			DetachFromSmosher();
			RespawnPlayer();
			return;
		}

		foreach (Collider2D killZone in killZones)
		{
			if (killZone != null && playerCollider.IsTouching(killZone))
			{
				DetachFromSmosher();
				RespawnPlayer();
				Debug.Log("Hello?");
				if (killZone.name == "KillZone1" && SceneManager.GetActiveScene().name == "main")
				{
					Debug.Log("I have died and should now be getting double jumps");
					playerController.doubleJumpCounter = 1;
					Debug.Log("Double jumps: " + playerController.doubleJumpCounter);
				}
				return;
			}
		}

		foreach (Collider2D waterfall in waterfalls)
		{
			if (waterfall != null && playerCollider.IsTouching(waterfall) && playerModifiers.GetWaterProof() == false)
			{
				DetachFromSmosher();
				RespawnPlayer();
				return;
			}
		}

		foreach (Collider2D gasZone in gasZones)
		{
			if (gasZone != null && playerCollider.IsTouching(gasZone) && playerModifiers.GetGasProof() == false)
			{
				DetachFromSmosher();
				RespawnPlayer();
				return;
			}
		}

		// Check if any box is near a killzone
		if (killZones != null && launchBoxBodies != null)
		{
			for (int i = 0; i < launchBoxBodies.Length; i++)
			{
				if (launchBoxBodies[i] == null) continue;
				Collider2D boxCol = launchBoxBodies[i].GetComponent<Collider2D>();
				if (boxCol == null) continue;

				Collider2D[] nearby = Physics2D.OverlapCircleAll(launchBoxBodies[i].position, 3f);

				foreach (Collider2D hit in nearby)
				{
					if (hit.CompareTag("KillZone"))
					{
						if (boxToLauncher.TryGetValue(launchBoxBodies[i], out BoxLauncher launcher))
							launcher.Flash();
						break;
					}
				}
			}
		}
	}

	// -------------------------------------------------------------------------
	// Screen transitions
	// -------------------------------------------------------------------------

	private void HandleTransitions()
	{
		if (transitions == null || currentScreen >= transitions.Length)
			return;

		Collider2D current = transitions[currentScreen];
		if (current == null || isTransitioning) return;

		if (playerCollider.IsTouching(current))
		{
			isTransitioning = true;

			int nextScreen = currentScreen + 1;
			if (nextScreen < level.spawnLocations.Length)
			{
				currentScreen = nextScreen;
				playerObject.transform.position = level.spawnLocations[currentScreen];
				Camera.main.transform.position = level.cameraPositions[currentScreen];
			}

			StartCoroutine(ResetTransitionFlag(current));
		}
	}

	private IEnumerator ResetTransitionFlag(Collider2D triggeredCollider)
	{
		triggeredCollider.enabled = false;
		yield return new WaitForFixedUpdate();
		triggeredCollider.enabled = true;
		isTransitioning = false;
	}

	// -------------------------------------------------------------------------
	// Respawn
	// -------------------------------------------------------------------------

	private void RespawnPlayer()
	{
		playerController.ResetPlayer();
		GameObject tempParticles = Instantiate(respawnParticles, level.spawnLocations[currentScreen], new Quaternion());
		tempParticles.GetComponent<ParticleSystem>().Play();
		// respawnParticles.transform.position = level.spawnLocations[currentScreen];
		RespawnLevel();
	}

	// -------------------------------------------------------------------------
	// Level loading
	// -------------------------------------------------------------------------

	public void LoadLevel()
	{
		StopAllActiveCoroutines();
		DetachFromSmosher();
		currentScreen = 0;
		globalBeatTimer = 0f;

		playerObject.transform.position = level.spawnLocations[currentScreen];
		Camera.main.transform.position = level.cameraPositions[currentScreen];

		launchedBoxes.Clear();
		ResolveLaunchBoxes();

		ResolveSmoshers();
		ResolveTransitions();
		ResolveKillZones();
		ResolveWaterfalls();
		ResolveGasZones();

		foreach (SmosherRuntime smosher in smosherRefs)
			TrackCoroutine(StartCoroutine(RunSmosher(smosher)));
	}

	private void RespawnLevel()
	{
		StopAllActiveCoroutines();
		DetachFromSmosher();
		globalBeatTimer = 0f;

		playerObject.transform.position = level.spawnLocations[currentScreen];
		Camera.main.transform.position = level.cameraPositions[currentScreen];

		ResetBoxes();

		ResolveSmoshers();
		foreach (SmosherRuntime smosher in smosherRefs)
			TrackCoroutine(StartCoroutine(RunSmosher(smosher)));
	}

	private void ResolveSmoshers()
	{
		smosherRefs.Clear();
		smosherWasMoving.Clear();

		foreach (SmosherData data in level.smoshers)
		{
			GameObject smosherObj = GameObject.Find(data.smosherName);
			if (smosherObj == null)
			{
				Debug.LogError($"Smosher not found: {data.smosherName}");
				continue;
			}

			Transform triggerTransform = smosherObj.transform.Find(data.smosherTriggerName);
			if (triggerTransform == null)
			{
				Debug.LogError($"Smosher trigger not found: {data.smosherTriggerName}");
				continue;
			}

			Collider2D stopCol = null;
			if (!string.IsNullOrEmpty(data.stopColliderName))
			{
				GameObject stopObj = GameObject.Find(data.stopColliderName);
				if (stopObj != null)
					stopCol = stopObj.GetComponent<Collider2D>();
				else
					Debug.LogWarning($"Stop collider not found: {data.stopColliderName}");
			}

			Collider2D[] allChildColliders = smosherObj.GetComponentsInChildren<Collider2D>();
			List<Collider2D> bodyColliders = new();
			foreach (Collider2D col in allChildColliders)
				if (!col.isTrigger)
					bodyColliders.Add(col);

			SmosherRuntime runtime = new SmosherRuntime
			{
				data = data,
				smosherObject = smosherObj,
				triggerObject = triggerTransform.gameObject,
				smosherColliders = bodyColliders.ToArray(),
				triggerCollider = triggerTransform.GetComponent<Collider2D>(),
				stopCollider = stopCol,
			};

			smosherRefs.Add(runtime);
			smosherWasMoving[runtime] = false;
		}
	}

	private void ResolveTransitions()
	{
		transitions = new Collider2D[level.screenTransitions.Length];
		for (int i = 0; i < transitions.Length; i++)
		{
			GameObject obj = GameObject.Find(level.screenTransitions[i]);
			if (obj == null)
			{
				Debug.LogError($"Transition not found: {level.screenTransitions[i]}");
				continue;
			}
			transitions[i] = obj.GetComponent<Collider2D>();
			if (transitions[i] == null)
				Debug.LogError($"Transition has no Collider2D: {level.screenTransitions[i]}");
		}
	}

	private void ResolveKillZones()
	{
		GameObject[] killZoneObjects = GameObject.FindGameObjectsWithTag("KillZone");
		killZones = new Collider2D[killZoneObjects.Length];
		for (int i = 0; i < killZoneObjects.Length; i++)
		{
			killZones[i] = killZoneObjects[i].GetComponent<Collider2D>();
			if (killZones[i] == null)
				Debug.LogWarning($"KillZone GameObject has no Collider2D: {killZoneObjects[i].name}");
		}
	}

	private void ResolveWaterfalls()
	{
		GameObject[] WaterfallObjects = GameObject.FindGameObjectsWithTag("waterfall");
		waterfalls = new Collider2D[WaterfallObjects.Length];
		for (int i = 0; i < WaterfallObjects.Length; i++)
		{
			waterfalls[i] = WaterfallObjects[i].GetComponent<Collider2D>();
			if (waterfalls[i] == null)
				Debug.LogWarning($"Waterfall GameObject has no Collider2D: {WaterfallObjects[i].name}");
		}
	}

	private void ResolveGasZones()
	{
		GameObject[] gasObjects = GameObject.FindGameObjectsWithTag("Gas");
		gasZones = new Collider2D[gasObjects.Length];
		for (int i = 0; i < gasObjects.Length; i++)
		{
			gasZones[i] = gasObjects[i].GetComponent<Collider2D>();
			if (gasZones[i] == null)
				Debug.LogWarning($"Gas GameObject has no Collider2D: {gasObjects[i].name}");
		}
	}

	// -------------------------------------------------------------------------
	// Coroutine managers
	// -------------------------------------------------------------------------

	private void TrackCoroutine(Coroutine c)
	{
		if (c != null) activeCoroutines.Add(c);
	}

	private void StopAllActiveCoroutines()
	{
		foreach (Coroutine c in activeCoroutines)
			if (c != null) StopCoroutine(c);
		activeCoroutines.Clear();
	}

	// -------------------------------------------------------------------------
	// Per-smosher coroutine
	// -------------------------------------------------------------------------

	private IEnumerator RunSmosher(SmosherRuntime smosher)
	{
		SmosherData d = smosher.data;
		Transform t = smosher.smosherObject.transform;

		t.position = d.topPosition;

		while (true)
		{
			// --- Drop phase ---
			float elapsed = 0f;
			bool reachedBottom = false;

			while (!reachedBottom)
			{
				elapsed += Time.deltaTime;
				float speed = elapsed * d.dropSpeed;

				bool hitStop = smosher.stopCollider != null
							   && smosher.smosherColliders != null
							   && smosher.smosherColliders.Length > 0
							   && smosher.smosherColliders[0] != null
							   && smosher.smosherColliders[0].IsTouching(smosher.stopCollider);

				if (hitStop)
				{
					Vector3 nudged = t.position + Vector3.up * 0.2f;
					while (Vector3.Distance(t.position, nudged) > 0.01f)
					{
						t.position = Vector3.MoveTowards(t.position, nudged, 2f * Time.deltaTime);
						yield return null;
					}
					reachedBottom = true;
				}
				else if (Vector3.Distance(t.position, d.bottomPosition) <= 0.01f)
				{
					t.position = d.bottomPosition;
					reachedBottom = true;
				}
				else
				{
					t.position = Vector3.MoveTowards(t.position, d.bottomPosition, speed * Time.deltaTime);
				}

				yield return null;
			}

			yield return new WaitForSeconds(d.waitAtBottom);

			// --- Rise phase ---
			bool reachedTop = false;
			while (!reachedTop)
			{
				if (Vector3.Distance(t.position, d.topPosition) <= 0.01f)
				{
					t.position = d.topPosition;
					reachedTop = true;

					Debug.Log($"Smosher {d.smosherName} reached top. riddenSmosher is {(riddenSmosher == null ? "null" : riddenSmosher.data.smosherName)}, this smosher is {d.smosherName}, match={riddenSmosher == smosher}");

					if (riddenSmosher == smosher)
					{
						Debug.Log("Smosher reached top — firing launch boost");
						DetachFromSmosher();
						ApplyLaunchBoost();
					}
				}
				else
				{
					t.position = Vector3.MoveTowards(t.position, d.topPosition, d.riseSpeed * Time.deltaTime);
				}
				yield return null;
			}

			yield return new WaitForSeconds(d.waitAtTop);
		}
	}

	// -------------------------------------------------------------------------
	// Smosher thud sound
	// -------------------------------------------------------------------------

	private void HandleSmosherThuds()
	{
		foreach (SmosherRuntime smosher in smosherRefs)
		{
			if (!smosher.data.playThudOnLand) continue;

			Transform t = smosher.smosherObject.transform;
			float distanceToBottom = Vector3.Distance(t.position, smosher.data.bottomPosition);
			bool nearBottom = distanceToBottom < SmosherBottomTolerance;

			float distanceToTop = Vector3.Distance(t.position, smosher.data.topPosition);
			bool isMoving = distanceToTop > 0.05f && distanceToBottom > SmosherStopThreshold;

			bool wasMoving = smosherWasMoving.ContainsKey(smosher) && smosherWasMoving[smosher];

			if (wasMoving && !isMoving && nearBottom && currentScreen == 0 && (smosher.data.smosherName == "Smosher0" || smosher.data.smosherName == "Smosher1" || smosher.data.smosherName == "Smosher2" || smosher.data.smosherName == "Smosher3")) {
				thudSound.Play();
			}
			if (wasMoving && !isMoving && nearBottom && currentScreen == 1 && (smosher.data.smosherName == "Smosher7" || smosher.data.smosherName == "Smosher8"))
			{
				thudSound.Play();
			}
			if (wasMoving && !isMoving && nearBottom && currentScreen == 0 && (smosher.data.smosherName == "Smosher9" || smosher.data.smosherName == "Smosher10" || smosher.data.smosherName == "Smosher11"))
			{
				thudSound.Play();
			}

			smosherWasMoving[smosher] = isMoving;
		}
	}

	// -------------------------------------------------------------------------
	// Launch boxes
	// -------------------------------------------------------------------------

	private void ResolveLaunchBoxes()
	{
		GameObject[] launchers = GameObject.FindGameObjectsWithTag("Launcher");
		launchBoxLaunchers = new BoxLauncher[launchers.Length];
		for (int i = 0; i < launchers.Length; i++)
			launchBoxLaunchers[i] = launchers[i].GetComponent<BoxLauncher>();

		GameObject[] boxes = GameObject.FindGameObjectsWithTag("Grabable");
		launchBoxBodies = new Rigidbody2D[boxes.Length];
		launchBoxStartPositions = new Vector3[boxes.Length];
		for (int i = 0; i < boxes.Length; i++)
		{
			launchBoxBodies[i] = boxes[i].GetComponent<Rigidbody2D>();
			launchBoxStartPositions[i] = boxes[i].transform.position;
		}
	}

	private void ResetBoxes()
	{
		if (launchBoxBodies == null) return;
		for (int i = 0; i < launchBoxBodies.Length; i++)
		{
			if (launchBoxBodies[i] == null) continue;
			launchBoxBodies[i].linearVelocity = Vector2.zero;
			launchBoxBodies[i].angularVelocity = 0f;
			launchBoxBodies[i].transform.position = launchBoxStartPositions[i];
		}
		launchedBoxes.Clear();
		boxToLauncher.Clear();
	}

	private void HandleLaunchBoxes()
	{
		if (launchBoxLaunchers == null || launchBoxBodies == null) return;

		if (killZones != null)
		{
			for (int i = 0; i < launchBoxBodies.Length; i++)
			{
				if (launchBoxBodies[i] == null) continue;
				Collider2D boxCol = launchBoxBodies[i].GetComponent<Collider2D>();
				if (boxCol == null) continue;

				foreach (Collider2D killZone in killZones)
				{
					if (killZone != null && boxCol.IsTouching(killZone))
					{
						launchBoxBodies[i].linearVelocity = Vector2.zero;
						launchBoxBodies[i].angularVelocity = 0f;
						launchBoxBodies[i].transform.position = launchBoxStartPositions[i];
						launchedBoxes.Remove(launchBoxBodies[i]);
						break;
					}
				}
			}
		}

		foreach (BoxLauncher launcher in launchBoxLaunchers)
		{
			if (launcher == null) { Debug.Log("LAUNCHER: launcher is null"); continue; }
			Collider2D launcherCol = launcher.GetComponent<Collider2D>();
			if (launcherCol == null) { Debug.Log("LAUNCHER: launcher has no Collider2D"); continue; }

			foreach (Rigidbody2D boxRb in launchBoxBodies)
			{
				if (boxRb == null) { Debug.Log("LAUNCHER: boxRb is null"); continue; }
				Collider2D boxCol = boxRb.GetComponent<Collider2D>();
				if (boxCol == null) { Debug.Log("LAUNCHER: box has no Collider2D"); continue; }

				bool touching = launcherCol.IsTouching(boxCol);

				if (touching && !launchedBoxes.Contains(boxRb))
				{
					launchedBoxes.Add(boxRb);
					boxToLauncher[boxRb] = launcher;
					boxRb.linearVelocity = Vector2.zero;
					boxRb.AddForce(launcher.launchDirection.normalized * launcher.launchForce, ForceMode2D.Impulse);
				}
				else if (!touching && launchedBoxes.Contains(boxRb))
				{
					launchedBoxes.Remove(boxRb);
				}
			}
		}
	}

	// -------------------------------------------------------------------------
	// Game Controllers
	// -------------------------------------------------------------------------

	public void ResetGame()
	{
		playerModifiers.ResetModification();
		LoadLevel();
	}

	public void OnEndZoneReached()
	{
		Debug.Log("End zone reached");
		timer.ResetTimer();
		playerModifiers.ResetModification();
		STATIC_DATA.NEXT_LEVEL = level.nextLevel;
		SceneManager.LoadScene("TransitionScreen");
	}
}