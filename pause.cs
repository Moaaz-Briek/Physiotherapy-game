using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class pause : MonoBehaviour
{
   public static bool isGamePaused=false;
    [SerializeField] GameObject pauseMenu;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)){

            if(isGamePaused){
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }
    public void ResumeGame(){
         pauseMenu.SetActive(false);
            Time.timeScale=1f;
            isGamePaused=false;

    }
    public void PauseGame()
    {
            pauseMenu.SetActive(true);
            Time.timeScale=0f;
            isGamePaused=true;

    }
 public void LoadMenu(){
     SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
      Time.timeScale=1f;
 } 
public void exit(){
    Application.Quit();
    Debug.Log("Quit");
}
}
