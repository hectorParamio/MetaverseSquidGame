using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlidingDoor : MonoBehaviour
{
    public Transform doorLeft;
    public Transform doorRight;
    public float slideDistance = 5f;
    public float speed = 3f;

    private Vector3 initialLeftPosition;
    private Vector3 initialRightPosition;
    private bool isOpening = false;

    void Start()
    {
        initialLeftPosition = doorLeft.localPosition;
        initialRightPosition = doorRight.localPosition;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            isOpening = !isOpening;
        }

        if (isOpening)
        {
            doorLeft.localPosition = Vector3.Lerp(doorLeft.localPosition, initialLeftPosition + new Vector3(-slideDistance, 0, 0), Time.deltaTime * speed);
            doorRight.localPosition = Vector3.Lerp(doorRight.localPosition, initialRightPosition + new Vector3(slideDistance, 0, 0), Time.deltaTime * speed);
        }
        else
        {
            doorLeft.localPosition = Vector3.Lerp(doorLeft.localPosition, initialLeftPosition, Time.deltaTime * speed);
            doorRight.localPosition = Vector3.Lerp(doorRight.localPosition, initialRightPosition, Time.deltaTime * speed);
        }
    }
}


