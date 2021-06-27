using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO.Ports;
using System.Threading;
public class Helicopter : MonoBehaviour
{
    // arduino with helicopter
    SerialPort serPort = new SerialPort("COM5", 9600);    
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
    public int last_direction = 0;
    public int result;
    public int counter = 0;

    void Start()
    {
        serPort.Open();
        serPort.ReadTimeout = 1;
    }


    void Update()
    {
        amountToMove = moveSpeed * Time.deltaTime;

        if (serPort.IsOpen)
        {
            try
            {
                Move_extension(serPort.ReadByte());
            }
            catch (System.Exception)
            {


            }
        }

        if (transform.position.y >= 3.4 && flag1)
        {
            validate.Push(Evalidate_up());
            flag2 = true;
            flag1 = false;
            Debug.Log("up");
            counter += 1;
            Debug.Log("counter : " + counter);
            Debug.Log("value : " + value);
        }

        if (transform.position.y <= -1.9 && flag2)
        {
            validate.Push(Evalidate_down());
            flag2 = false;
            flag1 = true;
            Debug.Log("down");
            counter += 1;
            Debug.Log("counter : " + counter);
            Debug.Log("value : " + value);
        }

        if (counter == 2)
        {
            if (Evalidate_exercise() >= 50)
            {
                SceneManager.LoadScene("accepted exercise");
            }
            else
            {
                SceneManager.LoadScene("rejected exercise");
            }
            Debug.Log("evalidate_exercise : " + evalidate_exercise);
        }
    }


    void Move_extension(int direction)
    {
        result = direction - last_direction;        
        if (Math.Abs(result) > 0)
        {            
            if (result > 0)
            {
                transform.Translate(Vector3.up * result * amountToMove, Space.World);
                data.Push(result);
                Debug.Log(result);
            }
            if(result < 0)
            {
                transform.Translate(Vector3.down * -1 * result * amountToMove, Space.World);
                data.Push(result);
                Debug.Log(result);
            }
        }
        last_direction = direction;                        
    }

    public bool Evalidate_up()
    {
        value = 0;
        while (!(data.Count == 0))
        {
            value += data.Pop();
        }

        value += 20;

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
        value -= 20;
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