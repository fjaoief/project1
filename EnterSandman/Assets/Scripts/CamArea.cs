using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ī�޶� ������. �÷��̾ �ڽ� �ݶ��̴� �ȿ� ���� ��� ī�޶��� ��ġ�� �� ������� ����.
/// </summary>
public class CamArea : MonoBehaviour
{
    BoxCollider2D boxCol2D;
    GameObject player;
    Vector3 targetPos;

    // Start is called before the first frame update
    void Start()
    {
        boxCol2D = GetComponent<BoxCollider2D>();   
        player = GameObject.Find("Player");
        targetPos = new Vector3(transform.position.x, transform.position.y, -10);
    }

    // Update is called once per frame
    void Update()
    {
        if (boxCol2D.bounds.Contains(player.transform.position))
        {
            Camera.main.transform.position = targetPos;
        }
    }
}
