using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float movementSpeed = 50f; // Speed of camera movement
    public float lookSpeed = 2f;     // Speed of camera rotation
    public float sprintSpeed = 2f;      // Speed multiplier for sprinting

    private void Start()
    {
        // Lock the cursor to the center of the screen and make it invisible
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        // Handle movement
        float moveX = Input.GetAxis("Horizontal"); // A/D or Left/Right Arrow
        float moveZ = Input.GetAxis("Vertical");   // W/S or Up/Down Arrow
        float moveY = 0;

        if (Input.GetKey(KeyCode.Space)) moveY = 2; // Move up
        if (Input.GetKey(KeyCode.C)) moveY = -2; // Move down

        Vector3 movement = new Vector3(moveX, moveY, moveZ);

        if (Input.GetKey(KeyCode.LeftShift))
        {
            // Sprinting
            movement *= sprintSpeed;
        }

        transform.Translate(movement * movementSpeed * Time.deltaTime, Space.Self);

        // Handle rotation
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        Vector3 rotation = transform.localEulerAngles;
        rotation.y += mouseX * lookSpeed;
        rotation.x -= mouseY * lookSpeed;
        transform.localEulerAngles = rotation;

        // Exit game when Escape is pressed
        if (Input.GetKeyDown(KeyCode.Escape))
        {
#if UNITY_EDITOR
            // Stop playing in the Unity Editor
            UnityEditor.EditorApplication.isPlaying = false;
#else
            // Quit the application in a build
            Application.Quit();
#endif
        }
    }
}
