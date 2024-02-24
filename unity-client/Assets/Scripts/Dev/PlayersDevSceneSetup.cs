using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayersDevSceneSetup : MonoBehaviour
{
    public GameManager GameManager;
    public PlayerController[] Player;

    // Start is called before the first frame update
    void Start()
    {
        Player[0].Init(GameManager, null);
        Player[1].Init(GameManager, null);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
