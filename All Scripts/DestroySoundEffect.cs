using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class DestroySoundEffect : MonoBehaviour
{
    public void Start()
    {
        Destroy(gameObject, 1);
    }
}
