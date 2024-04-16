using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyGameObjectAfterLifetime : MonoBehaviour
{
    public float lifetime = 1f;
    // Start is called before the first frame update
    void OnEnable()
    {
        Invoke("disable", lifetime);
    }

    void disable()
    {
        gameObject.SetActive(false);
    }
}
