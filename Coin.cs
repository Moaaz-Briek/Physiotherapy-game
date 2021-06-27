using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coin : MonoBehaviour
{

    private string coinType;
    public int coinValue;

    void Start()
    {
        /*coinType = name.Substring(0, 5);*/

        coinType = name;

        switch (coinType)
        {
            case "SilverCoin":
                coinValue = 5;
              
                break;
            case "GoldCoin":
                coinValue = 10;
             
                break;
            case "DiamondCoin":
                coinValue = 15;
               
                break;
                
        }

    }

    /*
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Helicopter"))
            ScoreCounter.instance.ChangeScore(coinValue);
    }*/
    
}
