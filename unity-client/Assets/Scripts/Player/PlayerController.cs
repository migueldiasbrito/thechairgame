using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private Action<PlayerController> _onReadyCallback;

    public void SetOnReadyCallback(Action<PlayerController> onReadyCallback)
    {
        _onReadyCallback = onReadyCallback;
    }

    public void OnPlayerReady(InputAction.CallbackContext callbackContext)
    {
        if (!callbackContext.performed) return;

        _onReadyCallback.Invoke(this);
    }
}
