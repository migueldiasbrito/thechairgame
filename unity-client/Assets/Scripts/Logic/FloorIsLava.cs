using UnityEngine;

public class FloorIsLava : MonoBehaviour
{
    [SerializeField] private GameManager _gameManager;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out PlayerController player))
        {
            _gameManager.PlayerFell(player);
        }
    }
}
