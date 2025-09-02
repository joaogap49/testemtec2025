using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Follow : MonoBehaviour
{
    public float MinModifier;
    public float MaxModifier;
    public Transform Target;
    Vector3 _velocity = Vector3.zero;
    bool _isFollowing = false;

    public void StartFollowing()
    {
        _isFollowing = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (_isFollowing)
        {
            transform.position = Vector3.SmoothDamp(transform.position, Target.position, ref _velocity, Time.deltaTime * Random.Range(MinModifier, MaxModifier));
        }
        
    }
}
