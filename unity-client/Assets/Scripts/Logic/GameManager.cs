using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public List<ChairController> Chairs { get; private set; } = new();
    [SerializeField] private float _intervalBetweenTurns = 5f;

    [SerializeField] private Vector2 _playerMinPosition = new Vector2(-1, -1);

    [SerializeField] private Vector2 _playerMaxPosition = new Vector2(1, 1);

    private List<PlayerController> _players = new();
    public GameState State { get; private set; } = GameState.Lobby;

    private Action _onStartTurnAction = null;

    public void SetStartTurnAction(Action onStartTurnAction)
    {
        _onStartTurnAction = onStartTurnAction;
    }

    public void AddPlayer(PlayerController player)
    {
        _players.Add(player);

        Vector3 playerPosition = player.transform.position;
        playerPosition.x = UnityEngine.Random.Range(_playerMinPosition.x, _playerMaxPosition.x);
        playerPosition.z = UnityEngine.Random.Range(_playerMinPosition.y, _playerMaxPosition.y);
        player.transform.position = playerPosition;

        //Chairs.Add(player.InitialChair);
    }

    public void AddChair(ChairController chair)
    {
        Chairs.Add(chair);
    }

    public void StartSetup()
    {
        State = GameState.Setup;
    }

    public void ChairPlaced()
    {
        if (_players.Where(x => x.InitialChair != null).Count() == 0)
        {
            StartNewTurn();
        }
    }

    private void StartTurn()
    {
        State = GameState.TurnStarted;
        _onStartTurnAction.Invoke();
    }

    public void EndTurn()
    {
        State = GameState.TurnEnded;
    }

    public void OnPlayerSit(PlayerController player)
    {
        switch(State)
        {
            case GameState.TurnStarted:
                EliminatePlayer(player);
                break;
            case GameState.TurnEnded:
                PlayerSittedOnTurnEnded();
                break;
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
        if (State == GameState.TurnStarted || State == GameState.TurnEnded)
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
    }

    private void PlayerSittedOnTurnEnded()
    {
        var _notSittedPlayers = _players.Where(x => x.ChairOccupied == null);

        if (_notSittedPlayers.Count() == 1)
        {
            EliminatePlayer(_notSittedPlayers.First());
        }
    }

    public void StartNewTurn()
    {
        int chairToDestroyIndex = UnityEngine.Random.Range(0, Chairs.Count);

        _players.Where(x => x.IsSitted).ToList().ForEach(x => x.GetUp());

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

    public void PlayerFell(PlayerController player)
    {
        if (player.InitialChair != null)
        {
            Chairs.Remove(player.InitialChair);
        }

        EliminatePlayer(player);
    }
}
