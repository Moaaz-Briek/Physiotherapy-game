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
        amountToMove = moveSpeed * Time.deltaTime;
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
            if (transform.position.y >= 3.1 && flag1)
            {
                flag2 = true;
                flag1 = false;
                Evalidate_up();
                flag_of_topDown += 1;
                //Debug.Log("Angle = 90");
            }

            if (transform.position.y <= -1.9 && flag2)
            {
                flag2 = false;
                flag1 = true;
                Evalidate_down();
                flag_of_topDown += 1;
                //Debug.Log("Angle = 0");
            }
        }
        else        
        {   if (Required_Exercises - Succeeded_Exercise == 0)
            {
                SceneManager.LoadScene("Accepted");
                remove_first_value_from_sensor = false;
                last_angle = 0;
            }
            else
            {
                SceneManager.LoadScene("Rejected");
                remove_first_value_from_sensor = false;
                last_angle = 0;
            }
        }
    }


    void Move_extension(int Angle)
    {
        Ratio_angle = last_angle - Angle;        
        if (Math.Abs(Ratio_angle) > 0 && Math.Abs(Ratio_angle) < 10  && remove_first_value_from_sensor)
        {
            Vector3 newPosition = transform.position; // We store the current position            
            float y = (Convert.ToSingle(0.061) * Convert.ToSingle(90 - Angle) ) - Convert.ToSingle(2.0);            
            newPosition.y = y;
            transform.position = newPosition; // We pass it back            
            data.Push(Ratio_angle);                         
        }

        last_angle = Angle;
        remove_first_value_from_sensor = true;
    }


    public void Evalidate_up()
    {
        value = 0;
        while (!(data.Count == 0))
        {
            value += data.Pop();            
        }
        if (value >= 80 && value <= 120)
        {
            Succeeded_Exercise += 1;            
        }
        else{}
        //Debug.Log(value);
    }

    public void Evalidate_down()
    {
        value = 0;
        while (!(data.Count == 0))
        {
            value += data.Pop();            
        }
        if (value <= -80 && value >= -120)
        {
            Succeeded_Exercise += 1;
        }
        else{}
        //Debug.Log(value);
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