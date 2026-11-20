using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbSpawner : MonoBehaviour
{
    // Start is called before the first frame update
    private OrbManager _orbManager;
    void Start()
    {
        _orbManager = GameObject.FindGameObjectWithTag("orbManager").GetComponent<OrbManager>();
    }

    // Update is called once per frame
    void OnTriggerEnter(Collider other)
    {
        _orbManager.SpawnOrbs();
    }
}
