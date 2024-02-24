using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Rigidbody _rigidbody;
    [SerializeField] private float _speed;

    private Vector2 _movement;

    private Action<PlayerController> _onReadyCallback;

    public void OnPlayerReady(InputAction.CallbackContext callbackContext)
    {
        if (!callbackContext.performed) return;

        _onReadyCallback.Invoke(this);
    }

    public void OnMove(InputAction.CallbackContext callbackContext)
    {
        _movement = callbackContext.ReadValue<Vector2>();
    }

    public void SetOnReadyCallback(Action<PlayerController> onReadyCallback)
    {
        _onReadyCallback = onReadyCallback;
    }

    private void FixedUpdate()
    {
        Vector3 velocity = _rigidbody.velocity;

        velocity.x = _movement.x * _speed * Time.fixedDeltaTime;
        velocity.z = _movement.y * _speed * Time.fixedDeltaTime;

        _rigidbody.velocity = velocity;
    }
}
