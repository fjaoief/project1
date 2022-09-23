using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 활성화 할 경우 카메라는 플레이어를 따라다님.
/// </summary>
public class PlayerCamera : MonoBehaviour
{
    public GameObject player;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (player != null)
        {
            transform.position = player.transform.position;
        }
    }
}
