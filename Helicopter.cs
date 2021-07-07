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
    public int value;
    public bool flag1 = true; //Top flag
    public bool flag2 = false; //Bottom flag    
    public int flag_of_topDown = 0; //This flag indicates how many times the helicopter is Top or Down
    public int Succeeded_Exercise = 0;
    public float y_flag_position = 0;

    //parameters from database
    public int Angle_of_exercise = 90; //This angle is set by doctor
    public int Required_Exercises = 2; //This value based on level number

    //parameters go to database
    public LinkedList<int> feel_pain_angle = new LinkedList<int>();
    public bool Result_of_exercise;
    public LinkedList<int> Flexion_Angle = new LinkedList<int>();
    public LinkedList<int> Extension_Angle = new LinkedList<int>();    

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

    public void FeelPain()
    {
        feel_pain_angle.AddLast(Angle);
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
            if (transform.position.y >= y_flag_position && flag1) // 0.745 == 45 degree ,, 3.5 == 90 degree.
            {                
                flag2 = true;
                flag1 = false;
                Flexion_Angle.AddLast(Evalidate());
                flag_of_topDown += 1;
                Debug.Log("Up");
                
            }

            if (transform.position.y <= -1.9 && flag2)
            {            
                flag2 = false;
                flag1 = true;
                Extension_Angle.AddLast(Evalidate());
                flag_of_topDown += 1;
                
            }
        }
        else        
        {   if (Required_Exercises - Succeeded_Exercise == 0)
            {
                SceneManager.LoadScene("Accepted");
                Result_of_exercise = true;
            }
            else
            {
                SceneManager.LoadScene("Rejected");
                Result_of_exercise = false;
            }

            last_angle = 0;
            remove_first_value_from_sensor = false;
            value = 0;

            Debug.Log("Angles The patient feel pain at : ");
            foreach (int angle in feel_pain_angle)
            {
                Debug.Log(angle + "\t");
            }
            
            Debug.Log("Flexion Angles : ");
            foreach (int angle in Flexion_Angle)
            {
                Debug.Log(angle + "\t");
            }
            
            Debug.Log("Extension Angles : ");
            foreach (int angle in Extension_Angle)
            {
                Debug.Log(angle + "\t");
            }
        }
    }


    void Move_extension(int Angle)
    {
        float y = (Convert.ToSingle(0.061) * Convert.ToSingle(90 - Angle)) - Convert.ToSingle(2.0); //This equation is the scale of the helicopter movemenet, 0.061 = ((Top - base) / 90)  
        if (Angle <= 90 && remove_first_value_from_sensor)
        {
            transform.position = new Vector3(transform.position.x, y, 0);
            data.Push(Math.Abs(last_angle - Angle));
            Debug.Log(Angle);
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