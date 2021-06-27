using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartGame : MonoBehaviour
{

   // void OnTriggerEnter2D(Collider2D collision)
   // {
       
   // }

 public void Startgame(){
     SceneManager.LoadScene("levels");
    
                    } 
public void LowLevel(){
     SceneManager.LoadScene("LowLevel");
    
                    } 
                    public void HighLevel(){
     SceneManager.LoadScene("HighLevel");
    
                    } 
 public void Exit(){
    Application.Quit();
    
                    }

}
