using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Ȱ��ȭ �� ��� ī�޶�� �÷��̾ ����ٴ�.
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
