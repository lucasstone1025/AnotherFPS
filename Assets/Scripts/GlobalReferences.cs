using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEditor;

public class GlobalReferences : MonoBehaviour
{
    public static GlobalReferences instance { get; set; }

    public GameObject bulletImpactEffectPrefab;
    public GameObject explosionEffectPrefab;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
    }



}
