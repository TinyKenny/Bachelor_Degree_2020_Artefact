using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Author: Hjalmar Andersson

public class PhysicsComponent : MonoBehaviour
{
    public Vector3 Velocity { get { return velocity; } set { velocity = value; } }

    [SerializeField] private float staticFriction;
    [SerializeField] private float dynamicFriction;
    [SerializeField] private float airResistance;
    [SerializeField] private Vector3 velocity = new Vector2(0, 0); //vibbens fel, make this private! (and property)


    public Vector3 AirFriction(Vector3 force)
    {
        force *= Mathf.Pow(airResistance, Time.deltaTime);
        return force;
    }

    public Vector3 GetVelocity()
    {
        return velocity;
    }
    public void SetVelocity(Vector3 newVel)
    {
        velocity = newVel;
    }

    public float GetStaticFriction()
    {
        return staticFriction;
    }
    public void SetStaticFriction(float newFric)
    {
        staticFriction = newFric;
    }

    public float GetAirResistance()
    {
        return airResistance;
    }
    public void SetAirResistance(float airForce)
    {
        airResistance = airForce;
    }

    public float GetDynamicFriction()
    {
        return staticFriction * 0.6f;
    }
}
