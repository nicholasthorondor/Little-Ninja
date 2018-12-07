using UnityEngine;

public class CameraController : MonoBehaviour {

    Vector3 offset;

	void Start () {
        // Calculates and assigns the offset distance between the player and the camera object.
        offset = transform.position - GameManager.instance.Player.transform.position;
	}
	
	void Update () {
        // Camera follows player position with offset applied.
        transform.position = GameManager.instance.Player.transform.position + offset;
	}
}
