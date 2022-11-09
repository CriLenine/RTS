using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Blueprint : MonoBehaviour
{
    private Vector3 _movePoint;
    public GameObject Prefab;
    void Start()
    {
        Vector2 worldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());

        if(Physics2D.OverlapPoint(worldPos))
        {
            transform.position = worldPos;
        }

    }

    void Update()
    {
        Vector2 worldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());

        if (Physics2D.OverlapPoint(worldPos))
        {
            transform.position = worldPos;
        }

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Instantiate(Prefab, transform.position, transform.rotation);
            Destroy(gameObject);
        }
    }
}
