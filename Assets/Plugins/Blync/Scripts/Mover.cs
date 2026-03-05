using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mover : MonoBehaviour
{
    CharacterController character;
    public FloatVariable BlyncSensorSpeed;
    public FloatVariable BlyncTurnAngle;
    public Transform front;
    public Transform bike;
    int direction = -1;
    float turnAngle = 0.0f, motion, verticalVelocity;
    Vector3 movement;
    Quaternion originalFrontRot;
    // Start is called before the first frame update
    void Start()
    {
        character = GetComponent<CharacterController>();
        originalFrontRot = front.transform.localRotation;
    }

    void FixedUpdate()
    {
        setTurnAngle();
        checkRotation();
        moveForward();
    }

    void checkRotation()
    {
        //rotate front bar
        front.transform.localRotation = originalFrontRot * Quaternion.Euler(0, direction * turnAngle, 0);
        if (BlyncSensorSpeed.value > 0f)
        {
            transform.Rotate(0, direction * Time.deltaTime * (turnAngle), 0, Space.Self);

        }
    }

    void setTurnAngle()
    {
        turnAngle = BlyncTurnAngle.value;
    }

    void moveForward()
    {
        motion = 0f;
        if (BlyncSensorSpeed.value > 0f)
        {
            motion = 1f;
        }

        movement = transform.TransformDirection(new Vector3(0, 0, motion));
        movement = movement * BlyncSensorSpeed.value;
        if (character.isGrounded)
        {
            verticalVelocity = 0;
        }
        else
        {
            verticalVelocity = 20f;
        }
        movement.y = verticalVelocity - (20 * Time.deltaTime);
        verticalVelocity = movement.y;
        character.Move(movement * Time.deltaTime);
    }
}
