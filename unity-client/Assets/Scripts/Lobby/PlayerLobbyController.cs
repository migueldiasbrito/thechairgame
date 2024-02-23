using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerLobbyController : MonoBehaviour
{
    [SerializeField] private PlayerInputManager _playerInputManager;

    [SerializeField] private TMP_Text _startInstruction;
    [SerializeField] private TMP_Text _noPlayersReadyLabel;
    [SerializeField] private TMP_Text _hasPlayersReadyLabel;
    [SerializeField] private TMP_Text _playersReadyLabel;

    private Dictionary<PlayerController, bool> _playersReady = new();

    public void OnPlayerJoined(PlayerInput playerInput)
    {
        if (playerInput.TryGetComponent(out PlayerController playerController))
        {
            _playersReady.Add(playerController, false);
            playerController.SetOnReadyCallback(OnPlayerReady);

            if (_playersReady.Count == 2)
            {
                _startInstruction.gameObject.SetActive(true);
                _noPlayersReadyLabel.gameObject.SetActive(true);
            }
        }
    }

    private void OnPlayerReady(PlayerController playerController)
    {
        if (_playersReady.Count < 2) return;

        if (_playersReady[playerController]) return;

        _playersReady[playerController] = true;
        _noPlayersReadyLabel.gameObject.SetActive(false);
        _hasPlayersReadyLabel.gameObject.SetActive(true);
        _playersReadyLabel.text = $"{_playersReady.Where(x => x.Value).ToList().Count} / {_playersReady.Count}";
        _playersReadyLabel.gameObject.SetActive(true);
    }
}
