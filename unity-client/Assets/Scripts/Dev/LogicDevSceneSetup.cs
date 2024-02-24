using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class LogicDevSceneSetup : MonoBehaviour
{
    public GameManager GameManager;
    public GameTurnWoMusic turnManager;

    public void OnPlayerJoined(PlayerInput playerInput)
    {
        if (playerInput.TryGetComponent(out PlayerController playerController))
        {
            GameManager.AddPlayer(playerController);
            playerController.Init(GameManager, OnPlayerReady);
        }
    }

    private void OnPlayerReady(PlayerController _)
    {
        GameManager.StartTurn();
    }

    // Start is called before the first frame update
    void Start()
    {
        GameManager.SetStartTurnAction(turnManager.StartTurn);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}