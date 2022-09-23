using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 카메라 에리어. 플레이어가 박스 콜라이더 안에 들어올 경우 카메라위 위치를 이 에리어로 변경.
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
