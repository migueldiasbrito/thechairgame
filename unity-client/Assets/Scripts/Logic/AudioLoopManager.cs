using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioLoopManager : MonoBehaviour
{
    [SerializeField] private GameManager _gameManager;

    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip _music;
    [SerializeField] private float _minDuration = 2f;

    private void Start()
    {
        _gameManager.SetStartTurnAction(StartTurn);
    }

    private void StartTurn()
    {
        float duration = Random.Range(_minDuration, _music.length);

        StartCoroutine(PlaySongFor(duration));
    }

    private IEnumerator PlaySongFor(float duration)
    {
        _audioSource.PlayOneShot(_music);
        yield return new WaitForSeconds(duration);

        _audioSource.Stop();
        _gameManager.EndTurn();
    }
}
