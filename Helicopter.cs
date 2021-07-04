using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Threading;
using System.IO;
using ArduinoBluetoothAPI;

public class Helicopter : MonoBehaviour
{
    private float amountToMove;
    
    // motion of the helicopter
    public float moveSpeed;
    public Rigidbody2D rb;
    private Vector2 moveDirection;
    
    //collect coins
    [SerializeField]
    public Text CoinCounter;
    public int CollidedCoinValue;
    public int TotalScore;
    public int LevelScore = 80;
    
    //Angle    
    public int Angle;
    public int last_angle = 0;
    public int Ratio_angle;
    public bool remove_first_value_from_sensor = false;    

    //Validation
    public Stack<int> data = new Stack<int>(); 
    public static int value;
    public bool flag1 = true; //Top flag
    public bool flag2 = false; //Bottom flag    
    public int flag_of_topDown = 0; //This flag indicates how many times the helicopter is Top or Down
    public int Succeeded_Exercise = 0;
    public float y_flag_position = 0;

    //parameters from database
    public int Angle_of_exercise = 45; //This angle is set by doctor
    public int Required_Exercises = 2; //This value based on level number
    
    //Object from the Arduino Bluetooth API Plugin
    public BluetoothHelper BTHelper;

    void Start()
    {        
        try
        {            
            BTHelper = BluetoothHelper.GetInstance("HC-05");         
            BTHelper.OnConnected += OnConnected;            
            BTHelper.OnConnectionFailed += OnconnFailed;
            BTHelper.setTerminatorBasedStream("\n");
            
            if(BTHelper.isDevicePaired()) // if we have already paired with the device
            {
                BTHelper.Connect(); // if we manage to connect successfully             
            }
        }

        catch (BluetoothHelper.BlueToothNotEnabledException ex) {}
        catch (BluetoothHelper.BlueToothNotSupportedException ex){}
        catch (BluetoothHelper.BlueToothNotReadyException ex){}
        y_flag_position = (Convert.ToSingle(0.061) * Convert.ToSingle(Angle_of_exercise) - Convert.ToSingle(2.0));
        Debug.Log("w");        
}

    void OnConnected()
    {
        BTHelper.StartListening(); //we start listening for incoming messages        
    }

    void OnconnFailed()
    {

    }

    void OnDestroy()
    {
        if (BTHelper != null)
        {
            BTHelper.Disconnect();            
        }
    }
    
    void Update()
    {        
        //amountToMove = moveSpeed * Time.deltaTime;
        if (BTHelper != null)
        {
            if (BTHelper.Available)
            {
                Angle = int.Parse(BTHelper.Read());                
                Move_extension(Angle);                
            }
        }

        if (flag_of_topDown < Required_Exercises)
        {            
            if (transform.position.y >= y_flag_position && flag1) // 0.745 == 45 degree ,, 
            {                
                flag2 = true;
                flag1 = false;
                Evalidate();
                flag_of_topDown += 1;
                Debug.Log("Up");
                
            }

            if (transform.position.y <= -1.9 && flag2)
            {            
                flag2 = false;
                flag1 = true;
                Evalidate();
                flag_of_topDown += 1;
                
            }
        }
        else        
        {   if (Required_Exercises - Succeeded_Exercise == 0)
            {
                SceneManager.LoadScene("Accepted");
                last_angle = 0;
            }
            else
            {
                SceneManager.LoadScene("Rejected");
                last_angle = 0;
            }
        }
    }


    void Move_extension(int Angle)
    {
        if (Angle <= 90 && remove_first_value_from_sensor)
        {
            transform.position = new Vector3(transform.position.x, (Convert.ToSingle(0.061) * Convert.ToSingle(90 - Angle)) - Convert.ToSingle(2.0), 0);            
            data.Push(Math.Abs(last_angle - Angle));
            Debug.Log(Angle);
        }
        else {}
        last_angle = Angle;
        remove_first_value_from_sensor = true;
    }


    public void Evalidate()
    {
        value = 0;
        while (!(data.Count == 0))
        {
            value += data.Pop();            
        }
        if (value >= (Angle_of_exercise - 10) && value <= (Angle_of_exercise + 10)) //tolerance
        {
            Succeeded_Exercise += 1;            
        }
        else{}
        Debug.Log("Evalidate : " + value);
    }
    
    void OnTriggerEnter2D(Collider2D collision)
    {
        CollidedCoinValue = collision.gameObject.GetComponent<Coin>().coinValue;
        TotalScore += CollidedCoinValue;

        if (collision.tag == "Coins")
        {
            Destroy(collision.gameObject);
        }

        if (collision.tag == "Diamond")
        {
            Destroy(collision.gameObject);
        }
    }


    IEnumerator waiter()
    {
        //Wait for 4 seconds
        yield return new WaitForSeconds(1);
    }
}