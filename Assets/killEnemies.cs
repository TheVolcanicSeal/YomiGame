using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class killEnemies : MonoBehaviour
{
    Collider2D killCollider;

    public float pausePerEnemy;
    private float remainingPausePerEnemy;
    public Queue<GameObject> enemiesToMurder;
    private bool executing;
    public Camera mainCamera;


    // Start is called before the first frame update
    void Start()
    {

        killCollider = GetComponent<CircleCollider2D>();

    }

    // Update is called once per frame
    void Update()
    {

        if (executing)
        {

            
            mainCamera.GetComponent<CameraFollow>().enabled = false;

            foreach (var item in enemiesToMurder)
            {
                remainingPausePerEnemy = pausePerEnemy;

                if (remainingPausePerEnemy > 0)
                {
                    remainingPausePerEnemy -= Time.unscaledDeltaTime;
                    mainCamera.transform.position =  new Vector3(Mathf.Lerp(mainCamera.transform.position.x, item.transform.position.x, remainingPausePerEnemy), Mathf.Lerp(mainCamera.transform.position.y, item.transform.position.y, remainingPausePerEnemy));

                }

                enemiesToMurder.Dequeue();

            }

            Time.timeScale = 1;

        }



    }

    private void OnTriggerEnter2D(Collider2D collision)
    {

        if (collision.tag == "Enemy")
        {

            enemiesToMurder.Enqueue(collision.gameObject);


        }

    }

    public void KillEnemies()
    {

        executing = true;
        Time.timeScale = 0;
        remainingPausePerEnemy = pausePerEnemy;




    }

}
