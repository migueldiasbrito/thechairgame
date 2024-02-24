using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameTurnWoMusic : MonoBehaviour
{
    public TMP_Text text;
    public float totaltime;
    public GameManager GameManager;

    private float currentTime;

    // Start is called before the first frame update
    void Start()
    {
        GameManager.SetStartTurnAction(StartTurn);
    }

    // Update is called once per frame
    void Update()
    {
        if (currentTime > 0)
        {
            currentTime -= Time.deltaTime;

            text.text = Mathf.CeilToInt(currentTime).ToString();

            if (currentTime <= 0)
            {
                GameManager.EndTurn();
            }
        }
    }

    public void StartTurn()
    {
        currentTime = totaltime;
    }
}
