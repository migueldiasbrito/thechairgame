using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [field: SerializeField] public List<ChairController> Chairs { get; private set; }
    [SerializeField] private float _intervalBetweenTurns = 5f;

    private List<PlayerController> _players = new();
    private GameState _gameState = GameState.Wait;

    private Action _onStartTurnAction = null;

    public void SetStartTurnAction(Action onStartTurnAction)
    {
        _onStartTurnAction = onStartTurnAction;
    }

    public void AddPlayer(PlayerController player)
    {
        _players.Add(player);
    }

    public void StartTurn()
    {
        _gameState = GameState.TurnStarted;
        _onStartTurnAction.Invoke();
    }

    public void EndTurn()
    {
        _gameState = GameState.TurnEnded;
    }

    public void OnPlayerSit(PlayerController player)
    {
        switch(_gameState)
        {
            case GameState.TurnStarted:
                EliminatePlayer(player);
                break;
            case GameState.TurnEnded:
                break;
            case GameState.Wait:
            default:
                break;
        }
    }

    private void EliminatePlayer(PlayerController player)
    {
        player.Eliminate();
        _players.Remove(player);

        OnPlayerEliminated();
    }

    private void OnPlayerEliminated()
    {
        if (_players.Count > 1)
        {
            StartNewTurn();
        }
        else
        {
            Debug.Log("WINNAH!");
        }
    }

    public void StartNewTurn()
    {
        int chairToDestroyIndex = UnityEngine.Random.Range(0, Chairs.Count);

        // Animate me instead...
        Destroy(Chairs[chairToDestroyIndex].gameObject);
        Chairs.RemoveAt(chairToDestroyIndex);

        StartCoroutine(ScheduleNextTurn());
    }

    private IEnumerator ScheduleNextTurn()
    {
        yield return new WaitForSeconds(_intervalBetweenTurns);

        StartTurn();
    }
}
