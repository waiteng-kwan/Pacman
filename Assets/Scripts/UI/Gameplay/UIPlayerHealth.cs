using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityAtoms.BaseAtoms;

public class UIPlayerHealth : MonoBehaviour
{
    public IntVariable CurrentHealth;
    public IntConstant StartingHealth;
    public IntConstant AbsoluteMaxHealth;

    public IntEvent EOnChangeEvent;

    private void Start()
    {
        //EOnChangeEvent.Register(OnHealthChanged);

        for(int i = 0; i < transform.childCount; i++)
            transform.GetChild(i).gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        //EOnChangeEvent.Unregister(OnHealthChanged);
    }

    public void OnHealthChanged()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            if(CurrentHealth.Value > i)
                transform.GetChild(i).gameObject.SetActive(true);
        }
    }
}
