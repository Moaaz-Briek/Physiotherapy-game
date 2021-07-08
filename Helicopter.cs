using System;
using System.Diagnostics;
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
    public float y_position;

    //Validation
    public Stack<int> data = new Stack<int>(); 
    public int value;
    public bool flag1 = true; //Top flag
    public bool flag2 = false; //Bottom flag    
    public int flag_of_topDown = 0; //This flag indicates how many times the helicopter is Top or Down
    public int Succeeded_Exercise = 0;
    public float Y_flag_position;
    
    //time
    public static Stopwatch stopWatch = new Stopwatch();
    public long ticksThisTime = 0;
    public bool flag = true;

    //parameters from database
    public int Angle_of_exercise = 90; //This angle is set by doctor
    public static int Required_Exercises = 2; //This value based on level number

    //parameters go to database
    public LinkedList<int> feel_pain_angle = new LinkedList<int>();
    public bool Result_of_exercise;

    public int[] Flexion_Angle = new int[Required_Exercises];
    public int[] Extension_Angle = new int[Required_Exercises];

    public double[] Flexion_Angle_time = new double[Required_Exercises];
    public double[] Extension_Angle_time = new double[Required_Exercises];

    public double[] Flexion_degree_per_second = new double[Required_Exercises];
    public double[] Extension_degree_per_second = new double[Required_Exercises];
    

    //Object from the ArduinoBluetoothAPI Plugin
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
        Y_flag_position = (Convert.ToSingle(0.061) * Convert.ToSingle(Angle_of_exercise) - Convert.ToSingle(2.0));        
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

    public void FeelPain()
    {
        feel_pain_angle.AddLast(Angle);
    }
        
    
    void Update()
    {            
        if (BTHelper != null)
        {
            if (BTHelper.Available)
            {
                Angle = int.Parse(BTHelper.Read());                
                Move_extension(Angle);                
            }
        }

        if (flag)
        {
            stopWatch.Reset();
            stopWatch.Start();
            flag = false;
        }


        if (flag_of_topDown < Required_Exercises)
        {
            if (transform.position.y >= Y_flag_position && flag1) // 0.745 == 45 degree ,, 3.5 == 90 degree.
            {
                flag2 = true;
                flag1 = false;
                flag = true;
                Flexion_Angle[flag_of_topDown] = Evalidate();                
                stopWatch.Stop();                
                Flexion_Angle_time[flag_of_topDown] = (stopWatch.ElapsedMilliseconds / 1000);
                flag_of_topDown += 1;
                UnityEngine.Debug.Log("Up" + flag_of_topDown);
            }

            if (transform.position.y <= -1.9 && flag2)
            {
                flag2 = false;
                flag1 = true;
                flag = true;
                Extension_Angle[flag_of_topDown - 1] = Evalidate();                                
                stopWatch.Stop();
                Extension_Angle_time[flag_of_topDown - 1] = (stopWatch.ElapsedMilliseconds / 1000);
                flag_of_topDown += 1;
                UnityEngine.Debug.Log("Up" + flag_of_topDown);
            }
        }
        else
        {
            if (Required_Exercises - Succeeded_Exercise == 0)
            {
                SceneManager.LoadScene("Accepted");
                Result_of_exercise = true;
                Database();
            }
            else
            {
                SceneManager.LoadScene("Rejected");
                Result_of_exercise = false;
                Database();
            }

            last_angle = 0;
            remove_first_value_from_sensor = false;
            value = 0;
        }
    }


    void Move_extension(int Angle)
    {        
        if (Angle <= 90 && remove_first_value_from_sensor)
        {
            transform.position = new Vector3(transform.position.x, (Convert.ToSingle(0.061) * Convert.ToSingle(90 - Angle)) - Convert.ToSingle(2.0), 0);
            data.Push(Math.Abs(last_angle - Angle));
            UnityEngine.Debug.Log(Angle);
        }
        else {}
        last_angle = Angle;
        remove_first_value_from_sensor = true;
    }


    public int Evalidate()
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
        return value;
    }
    
    void Database()
    {
        //Flexion_degree_per_second and Extension_degree_per_second parameters
        for (int index = 0; index < (Required_Exercises / 2); index++)
        {
            Flexion_degree_per_second[index] = Math.Round((Convert.ToSingle(Flexion_Angle[index]) / Flexion_Angle_time[index]), 2);
            UnityEngine.Debug.Log("Flexion Degree Per Second : " + Flexion_degree_per_second[index]);

            Extension_degree_per_second[index] = Math.Round((Convert.ToSingle(Extension_Angle[index]) / Extension_Angle_time[index]), 2);
            UnityEngine.Debug.Log("Extension Degree Per Second : " + Extension_degree_per_second[index]);
        }

        //Feel_Pain_Angle
        UnityEngine.Debug.Log("Angles The patient feel pain at : ");
        foreach (int angle in feel_pain_angle)
        {
            UnityEngine.Debug.Log(angle + "\t");
        }

        for (int index = 0; index < (Required_Exercises / 2); index++)
        {
            UnityEngine.Debug.Log("Flexion Angles : " + Flexion_Angle[index]);
            UnityEngine.Debug.Log("Extension Angles : " + Extension_Angle[index]);
        }

        //Total time
        double total_time = 0;
        for (int index = 0; index < (Required_Exercises / 2); index++)
        {
            total_time += Extension_Angle_time[index] + Flexion_Angle_time[index];            
        }
        UnityEngine.Debug.Log("Total time of Exercise : " + total_time + " second");

        //Result
        UnityEngine.Debug.Log("Result : " + Result_of_exercise);

        //Date of creation
        var dateTime = DateTime.Now;
        UnityEngine.Debug.Log(dateTime.ToShortDateString());
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