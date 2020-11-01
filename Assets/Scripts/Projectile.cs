﻿using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UI;
using System;
//using System.Diagnostics;
//using System.Diagnostics;

public class Projectile : MonoBehaviour
{
    #region Attributs

    //Joueur
    //public Joueur joueur;

    public static int points { get; set; } = 0;

    //Est attrapé par le joueur
    private bool isCaught;

    //Est lancé par le joueur
    private bool isLaunched;

    //vitesse de translation sur le tapis roulant
    public static float speedTapis = 5.0f;

    //Vitesse d'évolution de la force de lancer 
    private static float speedLancer = 1.0f;

    //vitesse de l'objet au lancer
    private float launchSpeed = 0;

    //Pour corriger la position de la souris sur l'écran
    private Vector3 mOffset;
    private float mZCoord;
    
    //Base temporelle pour calculer la force de lancer
    private float t0;

    //Pour l'animation de saisie
    private Animator animator;

    //FX
    public GameObject deathFX;
    public GameObject hitFX;
    public GameObject plusUnFX;
    public GameObject hitwaterFX;
    public GameObject wrongTargetFX;

    //Jauge de lancer
    public SpriteRenderer fleche;
    public Material material;

    

    #endregion



    // Start is called before the first frame update
    void Start()
    {
        isCaught = false;
        isLaunched = false;
        transform.Rotate(new Vector3(0.0f, 180f, 0.0f));
        animator = GetComponent<Animator>();

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        speedTapis *= 1.0001f;
        speedLancer *= 1.0001f;
        //Debug.Log(speedLancer + speedTapis);

        //Détruire l'objet si il dépasse une certaine position

        if (transform.position.x >= 6.0f && !isLaunched && !isCaught)
        {
            UnityEngine.Object.Destroy(this.gameObject);
        }

        
        //Les minions défilent sur le tapis s'ils ne sont pas attrapés et / ou lancés
        if (!isCaught && !isLaunched)
        {
            Vector3 direction = Vector3.Normalize(transform.position - new Vector3(10.0f, 0.0f, 0.0f));
            direction.y = 0.0f;
            direction.z = 0.0f;
            transform.Translate(direction * speedTapis * Time.deltaTime);
            
           
        }

        //lancement de l'animation si attrapé
        if (isCaught)
        {
            animator?.SetBool("Walk", true);
        }
    }

    void OnMouseDown()
    {
        
        //Le minion est attrapé
        isCaught = true;

        //On initialise un temps qui donnera la force de lancer
        t0 = Time.time;

        //Coordonnée z du minion
        mZCoord = Camera.main.WorldToScreenPoint(gameObject.transform.position).z;

        //offset = gameobject world pos - mouse world pos
        mOffset = gameObject.transform.position - GetMouseAsWorldPoint();

    }

    private Vector3 GetMouseAsWorldPoint()
    {
        //Coordonnées de la souris
        Vector3 mousePoint = Input.mousePosition;

        //Coordonnée z de la souris
        mousePoint.z = mZCoord;

        //Conversion avec l'échelle du world
        return Camera.main.ScreenToWorldPoint(mousePoint);
    }



    void OnMouseDrag()
    {
        //Drag le minion avec la souris
        transform.position = GetMouseAsWorldPoint() + mOffset;
        //Vitesse du shader
        material.SetFloat("_Yoffset", Mathf.Abs(Mathf.Sin(Time.time - t0))); //speedLancer * (Time.time - t0)
    }

    private void OnMouseUp()
    {
        //Le minion est laché et lancé...
        isCaught = false;
        isLaunched = true;

        //Variation sinusoidale de la force de lancer
        launchSpeed = Mathf.Abs(Mathf.Sin((Time.time-t0))*3000);//speedLancer * 

        //...dans la direction de la souris
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        transform.GetComponent<Rigidbody>().AddForce(ray.direction.x * launchSpeed, 500.0f, ray.direction.z * launchSpeed);
        Debug.Log(launchSpeed);
        material.SetFloat("_Yoffset",0.0f);
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        Quaternion rotation = Quaternion.Euler(0f, 0f, 0f);

        if (collision.gameObject.GetComponent("TapisRoulant") as TapisRoulant != null)
        {
            return;
        }
        else
        {
            if (collision.gameObject.GetComponent("Target") as Target != null)
            {
                if (collision.gameObject.name == "Target" + this.name)
                {
                    Instantiate(hitFX, new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z), Quaternion.identity);
                    Instantiate(plusUnFX, new Vector3(transform.position.x, 5.0f, transform.position.z), Quaternion.identity);
                    Debug.Log("You did  it!");
                    /*joueur.setPoints(joueur.getPoints() + 1);
                    Debug.Log(joueur.getPoints());*/
                    points += 1;
                    Debug.Log(points);
                    UnityEngine.Object.Destroy(this.gameObject);
                }
                else
                {
                    Instantiate(wrongTargetFX, new Vector3(transform.position.x, transform.position.y+1.0f, transform.position.z), Quaternion.identity);
                    Debug.Log("Wrong target!");
                    UnityEngine.Object.Destroy(this.gameObject);
                }


            }

            /*if (collision.gameObject.GetComponent("Target") as Target != null)
            {
                Instantiate(hitFX, new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z), Quaternion.identity);
                Instantiate(plusUnFX, new Vector3(transform.position.x, 5.0f, transform.position.z), Quaternion.identity);
                //joueur.setPoints(joueur.getPoints() + 1);
                //Debug.Log(joueur.getPoints());
                points += 1;
                UnityEngine.Object.Destroy(this.gameObject);
                
            }*/
            else if (collision.gameObject.GetComponent("Projectile") as Projectile != null)
            {
                Instantiate(hitFX, new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z), Quaternion.identity);
                return;
            }

            else if (collision.gameObject.GetComponent("Float") as Float != null)
            {
                Instantiate(hitFX, new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z), Quaternion.identity);
                return;
            }

            else if (collision.gameObject.GetComponent("Basket") as Basket != null)
            {
                Instantiate(hitFX, new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z), Quaternion.identity);
                return;
            }

            else if (collision.gameObject.GetComponent("Water") as Water != null)
            {
                Instantiate(hitwaterFX, new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z), rotation);
                Debug.Log("The water is lava!");
                UnityEngine.Object.Destroy(this.gameObject);
            }

            else
            {
                Instantiate(deathFX, new Vector3(transform.position.x, 1.0f, transform.position.z), rotation);
                Debug.Log("The floor is lava!");
                UnityEngine.Object.Destroy(this.gameObject);
                
            }
        }
    }
}
