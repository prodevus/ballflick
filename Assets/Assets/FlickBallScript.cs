using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FlickBallScript : MonoBehaviour {

	Vector3 ballInitPos = new Vector3(0,1.42f,0); // Reset ball to this position
	Vector3 cupPosition; // Position of goal cup
	Vector2 touchStartPos;
	Vector2 touchCurrentPos;

	Text ScoreText;
	Rigidbody rig;
	Transform FingerTrail;
	int score;
	
	List<Vector2> swipePositions = new List<Vector2>(); // Keep track of swipe positions to calculate effect

	float flickSpeed = 1;
	float resetTimer = 0;
	float effect = 0;


	private void Start() {
		rig = GetComponent<Rigidbody>();
		cupPosition = GameObject.Find("Cup").transform.position;
		ScoreText = GameObject.Find("Score").GetComponent<Text>();
		FingerTrail = GameObject.Find("FingerTrail").transform;
	}

	private void Update () {

		// Mobile inputs
		if(Input.touches.Length > 0) {
			touchCurrentPos = Input.touches[0].position;

			if(Input.touches[0].phase == TouchPhase.Began)
				touchStartPos = Input.touches[0].position;
			if(Input.touches[0].phase == TouchPhase.Ended || Input.touches[0].phase == TouchPhase.Canceled)
				FlickBall();
		}

		// Mouse inputs
		if(Input.GetMouseButtonDown(0))
			touchStartPos = Input.mousePosition;
		if(Input.GetMouseButton(0))
			touchCurrentPos = Input.mousePosition;
		if(Input.GetMouseButtonUp(0))
			FlickBall();

		// Keep track of positions for ball effect
		if(touchStartPos != Vector2.zero)
			swipePositions.Add(touchCurrentPos); // Add all points on swipe to a list of positions

		// How fast are we flicking the ball? Reduce timer the longer we hold the tap
		if(touchStartPos != Vector2.zero)
			flickSpeed -= Time.deltaTime;

		// If ball is in the air...
		if(resetTimer > 0) {
			resetTimer += Time.deltaTime;

			// Apply effect while in the air and fast enough
			if(rig.velocity.magnitude > 6)
				rig.AddForce(Vector3.left * effect / 15);

			// If the ball is near the ground and enough time has passed...
			if(resetTimer > 1 && transform.position.y < 0.5f) {
				// Check if we landed in cup
				if(Vector3.Distance(transform.position, new Vector3(cupPosition.x, transform.position.y, cupPosition.z)) < 0.5f) {
					score++;
					ScoreText.text = score.ToString();
				}
				// Reset ball position and velocity
				resetTimer = 0;
				transform.position = ballInitPos;
				rig.velocity = Vector3.zero;
				rig.isKinematic = true;
			}
		}

		// Finger trail graphics
		if(touchCurrentPos != Vector2.zero)
			FingerTrail.transform.position = Camera.main.ScreenToWorldPoint(new Vector3(touchCurrentPos.x, touchCurrentPos.y, Camera.main.nearClipPlane));

	}


	void FlickBall() {

		// Check if start touch is near the ball
		if(Vector2.Distance(touchStartPos, Camera.main.WorldToScreenPoint(transform.position)) < Screen.width/4) {

			// Calculate flick speed
			flickSpeed = Mathf.Clamp(flickSpeed, 0.1f, 1); // Clamp minimum and maximum flicking speed
			Vector3 swipeDir = (touchCurrentPos - touchStartPos) * flickSpeed; // The speed is based on how fast you release multiplied by the pixels your finger travels
			swipeDir *= (1920*1080)/(Screen.height*Screen.width); // Prevent different resolutions from affecting swipeDir speed! Alternatively, adjust for different phone sizes

			// Calculate effect
			effect = 0; // Reset effect from last time
			if(swipePositions.Count > 2) { // If we have enough points to calculate...
				Vector2 startDirection = Vector2.zero;
				// Find direction from start point to early point in swipe
				startDirection = swipePositions[Mathf.RoundToInt(swipePositions.Count/3)] - touchStartPos;
				// Find exit direction from end of swipe...
				Vector2 endDirection = swipePositions[swipePositions.Count-1] - swipePositions[swipePositions.Count-2]; 
				effect = Vector2.SignedAngle(startDirection, endDirection);
				effect = Mathf.Clamp(effect, -90, 90); // Clamp maximum effect
			}

			// If velocity is sufficient and ball is in reset position, flick ball
			if(swipeDir.magnitude > 50 && rig.isKinematic) { 
				rig.isKinematic = false;
				rig.AddForce(new Vector3(swipeDir.x, swipeDir.magnitude/2.1f, swipeDir.y)); // Swiping determines XZ velocity, Y velocity is based on how fast you swipe
				resetTimer = 0.1f;
			}
		}
			
		// Reset values
		touchStartPos = Vector2.zero;
		touchCurrentPos = Vector2.zero;
		flickSpeed = 1;
		swipePositions.Clear();
	}
		

}
