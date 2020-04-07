using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Main Author: Debatable

public class PlayerBaseState : MonoBehaviour
{
    #region Properties with set methods
    //values that will stay the same regardless of state
    protected Quaternion Rotation { get { return transform.rotation; } set { transform.rotation = value; } }
    protected Vector3 Velocity { get { return Velocity; } set { Velocity = value; } }
    protected Vector3 Direction { get { return Direction; } set { Direction = value; } }
    protected Vector3 LookDirection { get { return LookDirection; } set { LookDirection = value; } }
    protected Vector3 FaceDirection { get { return FaceDirection; } set { FaceDirection = value; } }
    protected float HorizontalDirection { get { return HorizontalDirection; } set { HorizontalDirection = value; } }
    protected float VerticalDirection { get { return VerticalDirection; } set { VerticalDirection = value; } }
   
    #endregion

    #region Properties with only get methods
    private CapsuleCollider CapsuleCollider { get { return GetComponent<CapsuleCollider>(); } }
    private LayerMask CollisionMask { get { return CollisionMask; } }
    protected Transform Position { get { return transform; } }


    //Physics Componetns
    protected PhysicsComponent OwnerPhysics
    {
        get { return GetComponent<PhysicsComponent>(); }
    }
    #endregion

    //values that can change from state to state

    private Vector3 pointUp;
    private Vector3 pointDown;
    private Vector3 runningDistance;
    private PhysicsComponent physics;
    private RaycastHit capsuleRaycast;
    private bool torchIsActive = false;
    private Vector3 SpawnPoint;
    private float frictionCoefficient = 0.95f;
    private float airFriction = 0.95f;

    protected float topSpeed = 8f;
    protected float jumpForce = 10f;
    protected float gravity = 6f;
    protected float skinWidth = 0.05f;
    protected float acceleration = 10f;
    protected float maxSpeed = 8f;
    protected bool isDead = false;
    protected float respawTimer = 0;
    

    public void Update()
    {
        CollisionCheck(Velocity * Time.deltaTime);
        Velocity = OwnerPhysics.AirFriction(Velocity);
       
        FaceTowardsDirection();

    }

    /// <summary>
    /// Faces the player towards the direction they are moving
    /// </summary>
    private void FaceTowardsDirection()
    {
        if (Velocity.magnitude > 0)
        {
            LookDirection = new Vector3(Velocity.x, 0, Velocity.z);
            Position.LookAt(LookDirection);
            FaceDirection += LookDirection * Time.deltaTime * 2;
            if (FaceDirection.magnitude > 1)
                FaceDirection = FaceDirection.normalized;
            Position.LookAt(Position.position + FaceDirection);

            //Position.rotation = Quaternion.RotateTowards(Position.rotation, Quaternion.LookRotation(LookDirection), 180.0f * Time.deltaTime);

        }
        else
        {

            //Position.rotation = Quaternion.RotateTowards(Position.rotation, Quaternion.LookRotation(LookDirection), 180.0f * Time.deltaTime);
            Position.LookAt(Position.position + FaceDirection);
            FaceDirection += LookDirection * Time.deltaTime * 2;
            if (FaceDirection.magnitude > 1)
                FaceDirection = FaceDirection.normalized;
            //Position.LookAt(Position.position + FaceDirection);
        }
        
    }

    
    /// <summary>
    /// Checks for collision using <see cref="capsuleRaycast"/> and recursive calls.
    /// </summary>
    /// <param name="frameMovement"></param>
    public void CollisionCheck(Vector3 frameMovement)
    {
        
        pointUp = Position.position + (CapsuleCollider.center + Vector3.up * (CapsuleCollider.height / 2 - CapsuleCollider.radius));
        pointDown = Position.position + (CapsuleCollider.center + Vector3.down * (CapsuleCollider.height / 2 - CapsuleCollider.radius));
        if (Physics.CapsuleCast(pointUp, pointDown, CapsuleCollider.radius, frameMovement.normalized, out capsuleRaycast, Mathf.Infinity, CollisionMask))
        {

            float angle = (Vector3.Angle(capsuleRaycast.normal, frameMovement.normalized) - 90) * Mathf.Deg2Rad;
            float snapDistanceFromHit = skinWidth / Mathf.Sin(angle);

            Vector3 snapMovementVector = frameMovement.normalized * (capsuleRaycast.distance - snapDistanceFromHit);

            //float snapdistance = capsuleRaycast.distance + skinWidth / Vector3.Dot(frameMovement.normalized, capsuleRaycast.normal);

            //Vector3 snapMovementVector = frameMovement.normalized * snapdistance;
            snapMovementVector = Vector3.ClampMagnitude(snapMovementVector, frameMovement.magnitude);
            Position.position += snapMovementVector;
            frameMovement -= snapMovementVector;

            Vector3 frameMovementNormalForce = HelpClass.NormalizeForce(frameMovement, capsuleRaycast.normal);
            frameMovement += frameMovementNormalForce;

            if (frameMovementNormalForce.magnitude > 0.001f)
            {
                Vector3 velocityNormalForce = HelpClass.NormalizeForce(Velocity, capsuleRaycast.normal);
                Velocity += velocityNormalForce;
                ApplyFriction(velocityNormalForce.magnitude);

            }

            if (frameMovement.magnitude > 0.001f)
            {
                CollisionCheck(frameMovement);
            }
            return;
        }

        else
        {
            Position.position += frameMovement;
        }
    }

    /// <summary>
    /// Lowers the players speed by a set amount each update.
    /// </summary>
    public void Decelerate()
    {
        pointUp = Position.position + (CapsuleCollider.center + Vector3.up * (CapsuleCollider.height / 2 - CapsuleCollider.radius));
        pointDown = Position.position + (CapsuleCollider.center + Vector3.down * (CapsuleCollider.height / 2 - CapsuleCollider.radius));
        Physics.CapsuleCast(pointUp, pointDown, CapsuleCollider.radius, Velocity.normalized, out capsuleRaycast, maxSpeed, CollisionMask);

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
        physics = collideObject.GetComponent<PhysicsComponent>();
        if (physics == null)
            return;
        normalForce = normalForce.normalized * (normalForce.magnitude + Vector3.Project(physics.GetVelocity(), normalForce.normalized).magnitude);
        Vector3 forceInDirection = Vector3.ProjectOnPlane(Velocity - physics.GetVelocity(), normalForce.normalized);
        Vector3 friction = -forceInDirection.normalized * normalForce.magnitude * OwnerPhysics.GetStaticFriction();

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
        
        pointUp = Position.position + (CapsuleCollider.center + Vector3.up * (CapsuleCollider.height / 2 - CapsuleCollider.radius));
        pointDown = Position.position + (CapsuleCollider.center + Vector3.down * (CapsuleCollider.height / 2 - CapsuleCollider.radius));
        Physics.CapsuleCast(pointUp, pointDown, CapsuleCollider.radius, Velocity.normalized, out capsuleRaycast, maxSpeed, CollisionMask);

        Vector3 velocityOnGround = Vector3.ProjectOnPlane(Velocity, capsuleRaycast.normal);

        float turnDot = Vector3.Dot(direction, velocityOnGround.normalized);

        if (velocityOnGround.magnitude > 0.001f && turnDot < 0.9f)
        {
            Velocity += Vector3.ClampMagnitude(direction, 1.0f) * 10 * acceleration * Time.deltaTime;
        }
        else
        {
            Velocity += Vector3.ClampMagnitude(direction, 1.0f) * acceleration * Time.deltaTime;
        }


        if (Velocity.magnitude > maxSpeed)
        {
            Velocity = Vector3.ClampMagnitude(new Vector3(Velocity.x, 0.0f, Velocity.z), maxSpeed) + Vector3.ClampMagnitude(new Vector3(0.0f, Velocity.y, 0.0f), 5.0f);
        }
        Position.rotation = Quaternion.Euler(direction.x, 0f, 0f);

        
        
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
        Vector3 pointUp = Position.position + CapsuleCollider.center + Vector3.up * (CapsuleCollider.height / 6 - CapsuleCollider.radius);
        Vector3 pointDown = Position.position + CapsuleCollider.center + Vector3.down * (CapsuleCollider.height / 6 - CapsuleCollider.radius);
        if (Physics.CapsuleCast(pointUp, pointDown, CapsuleCollider.radius, Vector3.down, out capsuleRaycast, (0.5f + skinWidth), CollisionMask)) // ändrade 0,8 till 0,6
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
        Vector3 pointUp = Position.position + CapsuleCollider.center + Vector3.up * (CapsuleCollider.height / 6 - CapsuleCollider.radius);
        Vector3 pointDown = Position.position + CapsuleCollider.center + Vector3.down * (CapsuleCollider.height / 6 - CapsuleCollider.radius);
        if (Physics.CapsuleCast(pointUp, pointDown, CapsuleCollider.radius, Vector3.down, out capsuleRaycast, (0.5f + skinWidth), CollisionMask))
        {
            return capsuleRaycast.normal;
        }
        else
        {
            return Vector3.zero;
        }
    }

    /// <summary>
    /// Respawns the player if killed.
    /// </summary>
 

    /// <summary>
    /// Applies friction to the player in order to slow down movement.
    /// </summary>
    /// <param name="normalForceMagnitude"></param>
    private void ApplyFriction(float normalForceMagnitude)
    {

        RaycastHit collision;
        Vector3 point1 = transform.position + CapsuleCollider.center + Vector3.up * (CapsuleCollider.height / 2 - CapsuleCollider.radius);
        Vector3 point2 = transform.position + CapsuleCollider.center + Vector3.down * (CapsuleCollider.height / 2 - CapsuleCollider.radius);



        if (Physics.CapsuleCast(point1, point2, CapsuleCollider.radius, Vector3.down, out collision, skinWidth * 5, CollisionMask))
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
        Direction = Camera.main.transform.rotation * new Vector3(HorizontalDirection, 0, VerticalDirection).normalized;
        //Direction = Vector3.ProjectOnPlane(Direction, GroundedNormal()).normalized;
    }

    /// <summary>
    /// Gives the movement variables values from input
    /// </summary>
    public void MovementInput()
    {
        VerticalDirection = Input.GetAxisRaw("Vertical");
        HorizontalDirection = Input.GetAxisRaw("Horizontal");

    }

    /// <summary>
    /// Updates the players direction to match the terrains normal.
    /// </summary>
    public void ProjectToPlaneNormal()
    {

        RaycastHit collision;
        Vector3 point1 = transform.position + CapsuleCollider.center + Vector3.up * (CapsuleCollider.height / 2 - CapsuleCollider.radius);
        Vector3 point2 = transform.position + CapsuleCollider.center + Vector3.down * (CapsuleCollider.height / 2 - CapsuleCollider.radius);

        Physics.CapsuleCast(point1, point2, CapsuleCollider.radius, Vector3.down, out collision, topSpeed, CollisionMask);

        Direction = Vector3.ProjectOnPlane(Direction, collision.normal).normalized;
        
    }

    /// <summary>
    /// Stops the player from sliding down hills
    /// </summary>
    public void ControlDirection()
    {
        //Vector3 pointUp = Position.position + CapsuleCollider.center + Vector3.up * (CapsuleCollider.height / 6 - CapsuleCollider.radius);
        //Vector3 pointDown = Position.position + CapsuleCollider.center + Vector3.down * (CapsuleCollider.height / 6 - CapsuleCollider.radius);
        //Physics.CapsuleCast(pointUp, pointDown, CapsuleCollider.radius, Vector3.down, out capsuleRaycast, topSpeed, CollisionMask);

        Vector3 projectedDirection = Vector3.ProjectOnPlane(Direction, capsuleRaycast.normal);
        if(Vector3.Dot(projectedDirection, Velocity) != 1)
        {
        Velocity = projectedDirection.normalized * Velocity.magnitude;
        }
    }

}
