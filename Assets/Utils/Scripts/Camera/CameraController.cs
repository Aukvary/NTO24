using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField]
    private float _cameraSpeed;

    [SerializeField]
    private float _rotateSpeed;

    [SerializeField] 
    private float _changeFOVSpeed;
    [SerializeField]
    private float _minFOV;

    [SerializeField]
    private float _maxFOV;

    private void Update()
    {
        Move();
        Rotate();
        Zooming();
    }

    private void Move()
    {
        if (!Input.GetKey(KeyCode.Mouse2) || Input.GetKey(KeyCode.LeftAlt))
            return;

        var xOffset = transform.right * -Input.GetAxis(�onstants.MouseX);
        var zOffset = transform.forward * -Input.GetAxis(�onstants.MouseY);

        transform.position = Vector3.Lerp(
            transform.position,
            transform.position + xOffset + zOffset,
            _cameraSpeed * Time.deltaTime);
    }

    private void Rotate()
    {
        if (!Input.GetKey(KeyCode.LeftAlt) || !Input.GetKey(KeyCode.Mouse2))
            return;
        transform
            .Rotate(0, Input.GetAxis(�onstants.MouseX) * _rotateSpeed, 0);
    }

    private void Zooming()
    {
        var newFow = Mathf.Clamp(Camera.main.fieldOfView - Input.mouseScrollDelta.y, _minFOV, _maxFOV);
        Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, newFow, Time.deltaTime * _changeFOVSpeed);
    }
}