using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinsMotion : MonoBehaviour
{[SerializeField] public Transform[] paths;
   [SerializeField] public float speed=2f;
    public int pathindex=0;   
    // Start is called before the first frame update
    void Start()
    {
       
       // StartCoroutine(CoinsRandomMotion());
    }

    // Update is called once per frame
    void Update()
    {
        
           move(); 
    }
private void move(){

        
if(pathindex <= paths.Length -1)
{ 
    //paths[].transform.position --> place of every point path
    //tranform.position --> coin position 
    transform.position = Vector2.MoveTowards(transform.position,paths[pathindex].transform.position, speed*Time.deltaTime);
 if (transform.position == paths[pathindex].transform.position){
     pathindex+=1;
   //  Debug.Log(transform.position);
     //Debug.Log(paths[pathindex].transform.position);
  } 
       
    }
    
    //else pathindex=0;
    }


   /* IEnumerator CoinsRandomMotion(){ // movment of coins with gravity from the sky
       yield return new WaitForSeconds(Random.Range(1,2));
       int randomCoins = Random.Range(0,coins.Length);
       if(Random.value<0.6f) 
    Instantiate(coins[randomCoins],new Vector2(Random.Range(-xBounds,xBounds),yBounds),Quaternion.identity);
    else 
    Instantiate(bomb,new Vector2(Random.Range(-xBounds,xBounds),yBounds),Quaternion.identity);
      
     
        StartCoroutine(CoinsRandomMotion());
      
    }*/

}