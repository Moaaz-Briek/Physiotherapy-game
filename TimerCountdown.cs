using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TimerCountdown : MonoBehaviour
{
    public GameObject textDisplay;
    public int secondsLeft = 30;
    public bool takingAway = false;

    // Start is called before the first frame update
    void Start()
    {
        textDisplay.GetComponent<Text>().text = secondsLeft + " Seconds Left";
    }

    // Update is called once per frame
    void Update()
    {
        if (takingAway == false && secondsLeft > 0)
            StartCoroutine( TimerTake() );

    }

    IEnumerator TimerTake()
    {
        takingAway = true;
        yield return new WaitForSeconds(1);
        secondsLeft -= 1;

        if (secondsLeft == 0)
        {
            textDisplay.GetComponent<Text>().text = "Time is over";
            SceneManager.LoadScene("rejected exercise");
        }
        else if (secondsLeft < 10 && secondsLeft > 0)
            textDisplay.GetComponent<Text>().text = "0" + secondsLeft + " Seconds Left";
        else
            textDisplay.GetComponent<Text>().text = secondsLeft + " Seconds Left";

        takingAway = false;
    }

    

}
