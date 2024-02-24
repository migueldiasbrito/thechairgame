using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioLoopManager : MonoBehaviour
{
    [SerializeField] private GameManager _gameManager;

    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip[] _audioClips;
    [SerializeField] private int _maxClipsPerTurns;

    private Queue<AudioClip> _currentTurnAudioClips = new();

    private void Start()
    {
        _gameManager.SetStartTurnAction(StartTurn);
    }

    private void StartTurn()
    {
        _currentTurnAudioClips.Clear();

        int numberOfClips = Random.Range(1, _maxClipsPerTurns + 1);

        for (int i = 0; i < numberOfClips; i++)
        {
            _currentTurnAudioClips.Enqueue(_audioClips[Random.Range(0, _audioClips.Length)]);
        }

        StartCoroutine(PlayNextSound());
    }

    private IEnumerator PlayNextSound()
    {
        while (_currentTurnAudioClips.Count > 0)
        {
            AudioClip clip = _currentTurnAudioClips.Dequeue();
            _audioSource.PlayOneShot(clip);
            yield return new WaitForSeconds(clip.length);
        }
        
        _gameManager.EndTurn();
    }
}
