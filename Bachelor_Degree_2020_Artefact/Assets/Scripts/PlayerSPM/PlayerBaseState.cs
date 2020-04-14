using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This class was ripped from a previous project and has been slightly adapted to our current needs.
// a lot of variables and methods are likely to be out of date
[RequireComponent(typeof(PhysicsComponent), typeof(CapsuleCollider))]
public class PlayerBaseState : MonoBehaviour
{
    public Vector3 Velocity = Vector3.zero;


    private Vector3 Direction = Vector3.zero;
    private Vector3 LookDirection = Vector3.zero;
    private Vector3 FaceDirection = Vector3.zero;




    [SerializeField] private LayerMask collisionMask = 0;
    
    private PhysicsComponent characterPhysics = null;
    private CapsuleCollider coll = null;

    
    private float horizontalDirection = 0.0f;
    private float verticalDirection = 0.0f;

    private Vector3 pointUp;
    private Vector3 pointDown;
    private PhysicsComponent otherPhysics;
    private RaycastHit capsuleRaycast;
    [SerializeField] private float frictionCoefficient = 0.95f;
    private float airFriction = 0.95f;

    private float topSpeed = 8f;
    private float gravity = 6f;
    private float skinWidth = 0.05f;
    private float acceleration = 10f;
    private float maxSpeed = 8f;


    private void Awake()
    {
        characterPhysics = GetComponent<PhysicsComponent>();
        coll = GetComponent<CapsuleCollider>();
    }

    public void Update()
    {
        if (Grounded())
        {
            MovementInput();

            CameraDirectionChanges();
            ProjectToPlaneNormal();
            ControlDirection();
            GroundDistanceCheck();
            Accelerate(Direction);
        }

        ApplyGravity();


        CollisionCheck(Velocity * Time.deltaTime);
        Velocity = characterPhysics.AirFriction(Velocity);

    }
    
    /// <summary>
    /// Checks for collision using <see cref="capsuleRaycast"/> and recursive calls.
    /// </summary>
    /// <param name="frameMovement"></param>
    public void CollisionCheck(Vector3 frameMovement)
    {
        
        pointUp = transform.position + (coll.center + Vector3.up * (coll.height / 2 - coll.radius));
        pointDown = transform.position + (coll.center + Vector3.down * (coll.height / 2 - coll.radius));
        if (Physics.CapsuleCast(pointUp, pointDown, coll.radius, frameMovement.normalized, out capsuleRaycast, Mathf.Infinity, collisionMask))
        {

            float angle = (Vector3.Angle(capsuleRaycast.normal, frameMovement.normalized) - 90) * Mathf.Deg2Rad;
            float snapDistanceFromHit = skinWidth / Mathf.Sin(angle);

            Vector3 snapMovementVector = frameMovement.normalized * (capsuleRaycast.distance - snapDistanceFromHit);

            //float snapdistance = capsuleRaycast.distance + skinWidth / Vector3.Dot(frameMovement.normalized, capsuleRaycast.normal);

            //Vector3 snapMovementVector = frameMovement.normalized * snapdistance;
            snapMovementVector = Vector3.ClampMagnitude(snapMovementVector, frameMovement.magnitude);
            frameMovement -= snapMovementVector;

            Vector3 frameMovementNormalForce = HelpClass.NormalizeForce(frameMovement, capsuleRaycast.normal);
            frameMovement += frameMovementNormalForce;

            transform.position += snapMovementVector;

            if (frameMovementNormalForce.magnitude > 0.001f)
            {
                Vector3 velocityNormalForce = HelpClass.NormalizeForce(Velocity, capsuleRaycast.normal);
                Velocity += velocityNormalForce;
                //ApplyFriction(velocityNormalForce.magnitude);

            }

            if (frameMovement.magnitude > 0.001f)
            {
                CollisionCheck(frameMovement);
            }
            return;
        }

        else
        {
            transform.position += frameMovement;
        }
    }

    /// <summary>
    /// Lowers the players speed by a set amount each update.
    /// </summary>
    public void Decelerate()
    {
        pointUp = transform.position + (coll.center + Vector3.up * (coll.height / 2 - coll.radius));
        pointDown = transform.position + (coll.center + Vector3.down * (coll.height / 2 - coll.radius));
        Physics.CapsuleCast(pointUp, pointDown, coll.radius, Velocity.normalized, out capsuleRaycast, maxSpeed, collisionMask);

        Vector3 velocityOnGround = Vector3.ProjectOnPlane(Velocity, capsuleRaycast.normal);
        Vector3 decelerationVector = velocityOnGround * frictionCoefficient;

        if(decelerationVector.magnitude > velocityOnGround.magnitude)
        {
            Velocity = Vector3.zero;
        }
        else
        {
            Velocity -= decelerationVector;
        }

     
    }

    /// <summary>
    /// Applies the velocity of a moving object onto the player if the player is standing on it.
    /// </summary>
    /// <param name="collideObject"></param>
    /// <param name="normalForce"></param>
    private void InheritVelocity(Transform collideObject, ref Vector3 normalForce)
    {
        otherPhysics = collideObject.GetComponent<PhysicsComponent>();
        if (otherPhysics == null)
            return;
        normalForce = normalForce.normalized * (normalForce.magnitude + Vector3.Project(otherPhysics.GetVelocity(), normalForce.normalized).magnitude);
        Vector3 forceInDirection = Vector3.ProjectOnPlane(Velocity - otherPhysics.GetVelocity(), normalForce.normalized);
        Vector3 friction = -forceInDirection.normalized * normalForce.magnitude * characterPhysics.GetStaticFriction();

        if (friction.magnitude > forceInDirection.magnitude)
            friction = friction.normalized * forceInDirection.magnitude;
        Velocity += friction;
    }

    /// <summary>
    /// Applies a constant force of gravity on the player.
    /// </summary>
    public void ApplyGravity()
    {
        //Velocity = Vector3.ProjectOnPlane(Velocity, owner.transform.right); //viktors kod som typ fungerade
        Velocity += Vector3.down * gravity * Time.deltaTime;
       
    }

    /// <summary>
    /// Gradually increases the players velocity.
    /// </summary>
    /// <param name="direction"></param>
    public void Accelerate(Vector3 direction)
    {

        //pointUp = transform.position + (coll.center + Vector3.up * (coll.height / 2 - coll.radius));
        //pointDown = transform.position + (coll.center + Vector3.down * (coll.height / 2 - coll.radius));
        //Physics.CapsuleCast(pointUp, pointDown, coll.radius, velocity.normalized, out capsuleRaycast, maxSpeed, collisionMask);

        //Vector3 velocityOnGround = Vector3.ProjectOnPlane(velocity, capsuleRaycast.normal);

        //float turnDot = Vector3.Dot(direction, velocityOnGround.normalized);

        //if (velocityOnGround.magnitude > 0.001f && turnDot < 0.9f)
        //{
        //    velocity += Vector3.ClampMagnitude(direction, 1.0f) * 10 * acceleration * Time.deltaTime;
        //}
        //else
        //{
        //    velocity += Vector3.ClampMagnitude(direction, 1.0f) * acceleration * Time.deltaTime;
        //}


        //if (velocity.magnitude > maxSpeed)
        //{
        //    velocity = Vector3.ClampMagnitude(new Vector3(velocity.x, 0.0f, velocity.z), maxSpeed) + Vector3.ClampMagnitude(new Vector3(0.0f, velocity.y, 0.0f), 5.0f);
        //}
        //transform.rotation = Quaternion.Euler(direction.x, 0f, 0f);



        Velocity = direction.normalized * maxSpeed;



        
    }

    /// <summary>
    /// Applies wind resistance to slow down player speed.
    /// </summary>
    public void AirFriction()
    {
        Velocity *= Mathf.Pow(0.95f, Time.deltaTime);
    }

    /// <summary>
    /// Uses a <see cref="capsuleRaycast"/> to see if the player has made contact with the cround
    /// </summary>
    /// <returns></returns>
    public bool Grounded()
    {
        Vector3 pointUp = transform.position + coll.center + Vector3.up * (coll.height / 2 - coll.radius);
        Vector3 pointDown = transform.position + coll.center + Vector3.down * (coll.height / 2 - coll.radius);
        if (Physics.CapsuleCast(pointUp, pointDown, coll.radius, Vector3.down, out capsuleRaycast, (0.05f + skinWidth), collisionMask)) // ändrade 0,8 till 0,6
        {
            return true;
        }
        else
            return false;
    }

    public void GroundDistanceCheck()
    {

        if (capsuleRaycast.collider != null)
        {
            if(capsuleRaycast.distance > 0.4f)
            {
                Velocity += new Vector3(0, -capsuleRaycast.distance * 5, 0);
                //Position.position += new Vector3(0, -capsuleRaycast.distance + 0.4f, 0);
            }
        }
       
    }

    /// <summary>
    /// Returns the normal angle of the object below the player.
    /// </summary>
    /// <returns></returns>
    public Vector3 GroundedNormal()
    {
        Vector3 pointUp = transform.position + coll.center + Vector3.up * (coll.height / 2 - coll.radius);
        Vector3 pointDown = transform.position + coll.center + Vector3.down * (coll.height / 2 - coll.radius);
        if (Physics.CapsuleCast(pointUp, pointDown, coll.radius, Vector3.down, out capsuleRaycast, (0.5f + skinWidth), collisionMask))
        {
            return capsuleRaycast.normal;
        }
        else
        {
            return Vector3.zero;
        }
    }

    /// <summary>
    /// Applies friction to the player in order to slow down movement.
    /// </summary>
    /// <param name="normalForceMagnitude"></param>
    private void ApplyFriction(float normalForceMagnitude)
    {

        RaycastHit collision;
        Vector3 point1 = transform.position + coll.center + Vector3.up * (coll.height / 2 - coll.radius);
        Vector3 point2 = transform.position + coll.center + Vector3.down * (coll.height / 2 - coll.radius);



        if (Physics.CapsuleCast(point1, point2, coll.radius, Vector3.down, out collision, skinWidth * 5, collisionMask))
        {
            {
              
                if (Velocity.magnitude < normalForceMagnitude * frictionCoefficient)
                {
                    Velocity = Vector3.zero;
                }
                else
                {
                    Vector3 temp = Velocity.normalized;
                    Velocity -= temp * normalForceMagnitude * frictionCoefficient * 0.9f; //tog bort * 0.8f;
                }
            }
        }
    }

    /// <summary>
    /// Also applies air friction.
    /// </summary>
    public void AirResistance()
    {
        Velocity *= Mathf.Pow(airFriction, Time.deltaTime);
    }
    
    /// <summary>
    /// Alters the direction input to match the cameras direction.
    /// </summary>
    public void CameraDirectionChanges()
    {
        Direction = Camera.main.transform.rotation * new Vector3(horizontalDirection, 0, verticalDirection).normalized;
        //Direction = Vector3.ProjectOnPlane(Direction, GroundedNormal()).normalized;
    }

    /// <summary>
    /// Gives the movement variables values from input
    /// </summary>
    public void MovementInput()
    {
        verticalDirection = Input.GetAxisRaw("Vertical");
        horizontalDirection = Input.GetAxisRaw("Horizontal");

    }

    /// <summary>
    /// Updates the players direction to match the terrains normal.
    /// </summary>
    public void ProjectToPlaneNormal()
    {

        RaycastHit collision;
        Vector3 point1 = transform.position + coll.center + Vector3.up * (coll.height / 2 - coll.radius);
        Vector3 point2 = transform.position + coll.center + Vector3.down * (coll.height / 2 - coll.radius);

        Physics.CapsuleCast(point1, point2, coll.radius, Vector3.down, out collision, topSpeed, collisionMask);

        Direction = Vector3.ProjectOnPlane(Direction, collision.normal).normalized;
        
    }

    /// <summary>
    /// Stops the player from sliding down hills
    /// </summary>
    public void ControlDirection()
    {
        //Vector3 pointUp = Position.position + CapsuleCollider.center + Vector3.up * (CapsuleCollider.height / 2 - CapsuleCollider.radius);
        //Vector3 pointDown = Position.position + CapsuleCollider.center + Vector3.down * (CapsuleCollider.height / 2 - CapsuleCollider.radius);
        //Physics.CapsuleCast(pointUp, pointDown, CapsuleCollider.radius, Vector3.down, out capsuleRaycast, topSpeed, CollisionMask);

        Vector3 projectedDirection = Vector3.ProjectOnPlane(Direction, capsuleRaycast.normal);
        if(Vector3.Dot(projectedDirection, Velocity) != 1)
        {
        Velocity = projectedDirection.normalized * Velocity.magnitude;
        }
    }

}
