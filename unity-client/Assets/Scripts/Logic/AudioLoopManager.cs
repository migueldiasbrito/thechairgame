using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioLoopManager : MonoBehaviour
{
    [SerializeField] private GameManager _gameManager;

    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip _music;
    [SerializeField] private float _minDuration = 2f;

    private Coroutine _coroutine;

    private void Start()
    {
        _gameManager.SetStartTurnAction(StartTurn);
    }

    private void StartTurn()
    {
        if (_coroutine != null)
        {
            _audioSource.Stop();
            StopCoroutine(_coroutine);
        }

        float duration = Random.Range(_minDuration, _music.length);

        _coroutine = StartCoroutine(PlaySongFor(duration));
    }

    private IEnumerator PlaySongFor(float duration)
    {
        _audioSource.PlayOneShot(_music);
        yield return new WaitForSeconds(duration);

        _audioSource.Stop();
        _gameManager.EndTurn();
    }
}
