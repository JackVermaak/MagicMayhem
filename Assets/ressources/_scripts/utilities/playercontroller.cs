using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playercontroller : MonoBehaviour
{
    private Transform _transform;
    [SerializeField]private float _speed = 1;
    // Start is called before the first frame update
    void Start()
    {
        _transform = transform;

    }

    // Update is called once per frame
    void Update()
    {
        Vector3 moveDirection = new Vector3 (Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        _transform.position = _transform.position + moveDirection * _speed * Time.deltaTime;


    }
}
