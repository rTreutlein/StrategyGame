using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.UI;

public class Unit : MonoBehaviour , ISelectable
{
    [Header ("Attributes")]
    public int max_health = 100;
    public int cost = 100;
    public int build_rate = 25;
    public string team;

    public bool ai;

    [Header("Unity Stuff")]
    public Image healthBar;
    public GameObject explosion;
    public GameObject wreck;
    public MonoBehaviour[] scripts;

    private int health = 0;
    private bool active = false;

    private Text txt;

    public bool Is_clicked { get; set; }
    public bool Is_selected { get; set; }

    public event EventHandler OnSelected;
    public event EventHandler OnDeselected;
    public event EventHandler OnActivate;

    // Start is called before the first frame update
    void Start()
    {
        GameObject ui = GameObject.FindObjectOfType<InputManager>().ui;
        txt = ui.transform.Find("txtName").GetComponent<Text>();
    }

    public void Damage(int damage)
    {
        Debug.Log("Took Damage!");
        health -= damage;
        healthBar.fillAmount = health / max_health;

        if (health <= 0)
        {
            GameObject exp_inst = Instantiate(explosion, transform.position, transform.rotation);
            ParticleSystem psys = exp_inst.GetComponent<ParticleSystem>();
            float total_duration = psys.main.duration;

            GameObject wreck_inst = Instantiate(wreck, transform.position, transform.rotation);
            wreck_inst.GetComponent<Wreck>().metal = Mathf.RoundToInt(cost * 0.8f);

            Destroy(exp_inst, total_duration);
            Destroy(gameObject);
        }
    }
    
    public void AddHealth(int ammount)
    {
        health = +ammount;
    }    

    public void Activate()
    {
        active = true;
        foreach (var script in scripts)
        {
            script.enabled = true;
        }
        OnActivate?.Invoke(this, EventArgs.Empty);
    }

    bool ISelectable.Deselect()
    {
        if (Is_clicked)
            return false;

        OnDeselected?.Invoke(this, EventArgs.Empty);

        Is_selected = false;
        txt.text = "";
        return true;
    }

    bool ISelectable.CanDeselect()
    {
        return !Is_clicked;
    }

    void ISelectable.Select()
    {
        Is_selected = true;
        txt.text = gameObject.name;
        OnSelected?.Invoke(this, EventArgs.Empty);
    }
}
