using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PawnBase : MonoBehaviour
{
    //serialize properties use field:
    [field: SerializeField]
    public int Index { get; protected set; } = -1;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
