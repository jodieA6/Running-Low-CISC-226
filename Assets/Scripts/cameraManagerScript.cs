// using Unity.VisualScripting;
// using UnityEngine;

// public class cameraManagerScript : MonoBehaviour
// {

//     [SerializeField] GameObject playerObject;
//     [SerializeField] Camera cam;
//     public GameObject topLeftBound;
//     public GameObject bottomRightBound;
//     private float camWidth;
//     private float camHeight;

//     // Start is called once before the first execution of Update after the MonoBehaviour is created
//     void Start()
//     {
//         cam = GetComponent<Camera>();
//         camHeight = cam.orthographicSize; // half of cam height
//         camWidth = cam.orthographicSize * cam.aspect; // half of cam width
//     }

//     // Update is called once per frame
//     void Update()
//     {
//         Vector3 ppos = playerObject.transform.position;
//         Vector3 tlbpos = topLeftBound.transform.position;
//         Vector3 brbpos = bottomRightBound.transform.position;
//         float targetX = Mathf.Clamp(ppos.x, tlbpos.x + camWidth, brbpos.x - camWidth);
//         float targetY = Mathf.Clamp(ppos.y, brbpos.y + camHeight, tlbpos.y - camHeight);

//         this.transform.position = new Vector3(targetX, targetY, this.transform.position.z);
//     }
// }
