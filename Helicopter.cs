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

    public Stack<int> data = new Stack<int>();
    public static int value;
    public bool flag1 = true; //up
    public bool flag2 = false; //down
    public Stack<bool> validate = new Stack<bool>();
    public int evalidate_exercise;
    public int Angle;
    public int last_angle = 0;
    public int Ratio_angle;
    public bool remove_first_value_from_sensor = false;
    public int number_of_flexion_and_extension_exercises = 0;
    public int number_of_flexion_and_extension_exercises_come_from_doctor = 2;
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

        catch (BluetoothHelper.BlueToothNotEnabledException ex) { Debug.Log("1"); }
        catch (BluetoothHelper.BlueToothNotSupportedException ex){ Debug.Log("1"); }
        catch (BluetoothHelper.BlueToothNotReadyException ex){ Debug.Log("1"); }
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
                Debug.Log(Angle);
                Move_extension(Angle);                
            }
        }                
        
        if (transform.position.y >= 3.1 && flag1)
        {
            validate.Push(Evalidate_up());
            flag2 = true;
            flag1 = false;

            Debug.Log("up");
            number_of_flexion_and_extension_exercises += 1;
            //Debug.Log("number_of_flexion_and_extension_exercises : " + number_of_flexion_and_extension_exercises);
            Debug.Log("value : " + value);
        }

        if (transform.position.y <= -1.9 && flag2)
        {
            validate.Push(Evalidate_down());
            flag2 = false;
            flag1 = true;

            Debug.Log("down");
            number_of_flexion_and_extension_exercises += 1;
            //Debug.Log("number_of_flexion_and_extension_exercises : " + number_of_flexion_and_extension_exercises);
            Debug.Log("value : " + value);
        }

        if (number_of_flexion_and_extension_exercises == number_of_flexion_and_extension_exercises_come_from_doctor)
        {
            if (Evalidate_exercise() >= 50)
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
            if (Ratio_angle > 0)
            {
                transform.Translate(Vector3.up * Ratio_angle * amountToMove, Space.World);
                data.Push(Ratio_angle);
                //Debug.Log(result);
            }
            if (Ratio_angle < 0)
            {
                int positive_ratio_angle = -1 * Ratio_angle;
                transform.Translate(Vector3.down * positive_ratio_angle * amountToMove, Space.World);
                data.Push(Ratio_angle);
                //Debug.Log(result);
            }
        }
        last_angle = Angle;
        remove_first_value_from_sensor = true;
    }

    public bool Evalidate_up()
    {
        value = 0;
        while (!(data.Count == 0))
        {
            value += data.Pop();
        }
        if (value >= 80 && value <= 120)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool Evalidate_down()
    {
        value = 0;
        while (!(data.Count == 0))
        {
            value += data.Pop();
        }
        if (value <= -80 && value >= -120)
        {
            return true;     
            
        }
        else
        {
            return false;
        }
    }

    public int Evalidate_exercise()
    {
        evalidate_exercise = 0;
        while (!(validate.Count == 0))
        {
            if (validate.Pop())
            {
                evalidate_exercise += 25;
            }
            else
            {
                evalidate_exercise += 0;
            }
        }
        return evalidate_exercise;

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