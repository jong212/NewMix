using UnityEngine;

public class IsometricCameraPanner : MonoBehaviour
{
    public float panSpeed = 6f;

    void Update()
    {
        Vector2 panPosition = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

        Debug.Log($"Horizontal: {panPosition.x}, Vertical: {panPosition.y}"); // Test to see if input is detected

        transform.position += new Vector3(panPosition.x, 0, panPosition.y) * (panSpeed * Time.deltaTime);
    }
}
