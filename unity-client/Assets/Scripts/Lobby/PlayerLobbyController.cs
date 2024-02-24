using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerLobbyController : MonoBehaviour
{
    [SerializeField] private PlayerInputManager _playerInputManager;
    [SerializeField] private GameManager _gameManager;

    [SerializeField] private GameObject _lobbyPanel;
    [SerializeField] private GameObject _startInstruction;
    [SerializeField] private GameObject _noPlayersReadyLabel;
    [SerializeField] private GameObject _hasPlayersReadyLabel;
    [SerializeField] private TMP_Text _playersReadyLabel;

    private Dictionary<PlayerController, bool> _playersReady = new();

    public void OnPlayerJoined(PlayerInput playerInput)
    {
        if (playerInput.TryGetComponent(out PlayerController playerController))
        {
            _playersReady.Add(playerController, false);
            playerController.Init(_gameManager, OnPlayerReady);

            if (_playersReady.Count == 2)
            {
                _startInstruction.SetActive(true);
                _noPlayersReadyLabel.SetActive(true);
            }
        }
    }

    private void OnPlayerReady(PlayerController playerController)
    {
        if (_playersReady.Count < 2) return;

        if (_playersReady[playerController]) return;

        int numberOfPlayersReady = _playersReady.Where(x => x.Value).ToList().Count;

        if (numberOfPlayersReady == _playersReady.Count)
        {
            _lobbyPanel.SetActive(false);
            _playerInputManager.DisableJoining();
        }
        else
        {
            _playersReady[playerController] = true;
            _noPlayersReadyLabel.SetActive(false);
            _hasPlayersReadyLabel.SetActive(true);
            _playersReadyLabel.text = $"{numberOfPlayersReady} / {_playersReady.Count}";
            _playersReadyLabel.gameObject.SetActive(true);
        }
    }

    private void Start()
    {
        _lobbyPanel.SetActive(true);
    }
}
