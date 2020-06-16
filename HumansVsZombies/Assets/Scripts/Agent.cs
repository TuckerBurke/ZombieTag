using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Agent : MonoBehaviour
{
    // Local variables
    public Vector3 parkCenter = new Vector3(0, 0.12f, 0); // Center of the park
    public Vector3 agentPosition;   // Agents current position     
    public Vector3 acceleration;    // Total change in speed due to forces
    public Vector3 direction;       // Objects heading
    public Vector3 velocity;        // Speed and direction
    public Vector3 forward;         // Objects heading
    public Vector3 right;           // -90 degrees of heading
    public Vector3 futurePosition;  // Used for evade / pursue
    public float mass;              // Objects mass for force calculations
    public float radius;            // Game object radius for collision
    public float maxSpeed;          // Speed scalar
    public float wanderingSpeed;    // Wandering scalar
    public float verticleOffset;    // Offset due to 3D anchor
    public float weightBounds;      // Force applied near edge of park
    public float weightCentral;     // Force applied in center of park
    public Material agentForward;   // Material for debug line
    public Material agentRight;     // Material for debug line
    public GameObject sceneManager; // Reference to the game manager
    public float separatedDistance; // Distance to separate like game objects
    public float separationWeight;  // Separation force scalar
    public float separationSpeed;   // Separation velocity scalar
    public bool wandering = false;  // State
    float wanderingAngle;           // Stored for smooth offsetting
    public GameObject[] trees;      // Array of obsticles
    public float safeDistance;      // Distance to check for obsticles

    // Use this for initialization ******************************************************************
    void Start ()
    {
        // Obtain a reference to the scene manager
        sceneManager = GameObject.FindGameObjectWithTag("Manager");
        
        // Obtain the game object's verticle offset
        verticleOffset = sceneManager.GetComponent<Management>().verticleOffset;

        // Calculate forward and right to account for Y offset
        CalcLocalAxis();

        // Store direction
        direction = forward;

        // Calculate initial wandering angle
        wanderingAngle = Random.Range(0, 360);

        // Gather references to obsticles
        trees = GameObject.FindGameObjectsWithTag("Obsticle");
    }

    // Calculate local axis to account for Y offset *************************************************
    protected virtual void CalcLocalAxis ()
    {
        // Correct forward axis
        forward = transform.forward + transform.position;

        // Correct right axis
        right = transform.right + transform.position;
    }

    // Draws debug lines ***************************************************************************
    protected virtual void OnRenderObject ()
    {
        // Calculate forward and right to account for Y offset
        CalcLocalAxis();

        // If in debug mode
        if (sceneManager != null && sceneManager.GetComponent<Management>().debugMode)
        {
            // Draw line for agents forward vector
            agentForward.SetPass(0);
            GL.Begin(GL.LINES);
            GL.Vertex(transform.position);
            GL.Vertex(forward);
            GL.End();

            // Draw line for agents right vector
            agentRight.SetPass(0);
            GL.Begin(GL.LINES);
            GL.Vertex(transform.position);
            GL.Vertex(right);
            GL.End();
        }
    }

    // Executes vector movement via velocity and acceleration **************************************
    protected virtual void Move ()
    {
        // Store initial position
        agentPosition = transform.position;

        // Apply standard movement
        velocity += acceleration * Time.deltaTime;
        velocity = Vector3.ClampMagnitude(velocity, maxSpeed);
        agentPosition += velocity * Time.deltaTime;
        direction = velocity.normalized;
        transform.position = agentPosition;

        // Reset variables
        acceleration = Vector3.zero;
    }

    // Applies physics to acceleration vector ****************************************************
    protected virtual void ApplyForce(Vector3 force)
    {
        acceleration += force / mass;
        acceleration.y = 0;
    }

    // Calculate force to remain within park bounds *********************************************
    protected virtual void Boundaries ()
    { 
        // Calculate desired velocity
        Vector3 desiredVelocity = parkCenter - agentPosition;

        // If near the edge of the park
        if (desiredVelocity.magnitude >= 3.75f)
        {
            // Scale desired velocity based by weight
            desiredVelocity = Vector3.ClampMagnitude(desiredVelocity, weightBounds);

            // Calculate seeking force
            Vector3 force = desiredVelocity - velocity;

            // Rotate agent
            //transform.forward = desiredVelocity * Time.deltaTime;
            Vector3 newDir = Vector3.RotateTowards(transform.forward, desiredVelocity, maxSpeed * Time.deltaTime, 0.0f);
            transform.rotation = Quaternion.LookRotation(newDir);

            // Apply force
            ApplyForce(force);
        }

        // If near the middle of the park
        if (desiredVelocity.magnitude < 3.75f)
        {
            // Scale desired velocity based on max speed
            desiredVelocity = Vector3.ClampMagnitude(desiredVelocity, weightCentral);

            // Calculate seeking force
            Vector3 force = desiredVelocity - velocity;

            // Apply force
            ApplyForce(force);
        }
    }

    // Calculates separation force **************************************************************
    protected virtual void Separate (GameObject[] likeObjects)
    {
        // For each object
        for (int h = 0; h < likeObjects.Length; h++)
        {
            // Calculate distance between this object and current array object
            Vector3 currentToThis = gameObject.transform.position - likeObjects[h].transform.position;
            float distance = currentToThis.magnitude;

            // If the distance is not zero (otherwise object is checking itself)
            if (distance != 0)
            {
                // If distance is less than desired distance
                if (distance < separatedDistance)
                {
                    // Calculate desired velocity
                    Vector3 desiredVelocity = currentToThis;

                    // Scale desired velocity
                    desiredVelocity.Normalize();
                    desiredVelocity = desiredVelocity / distance;

                    // Rotate agent *Note: design choice see doc
                    transform.forward = desiredVelocity * Time.deltaTime;

                    // Calculate the force 
                    Vector3 steeringForce = desiredVelocity - velocity;

                    // Apply steering force
                    ApplyForce(steeringForce * separationWeight);
                }
            }

        }
    }

    // Calculates obsticle avoidance force *****************************************************
    protected virtual void Avoid ()
    {
        // For each obsticle
        for (int t = 0; t < trees.Length; t++)
        {
            // Create vector from agent to obsticle
            Vector3 agentToTree = trees[t].transform.position - transform.position;

            // If the obsticle is getting close and is in front of agent
            if ((agentToTree.magnitude < safeDistance) && (Vector3.Dot(forward, agentToTree) > 0))
            {
                // Check for potential collision
                // If projection magnetude onto right vector is less than sum of radii
                if(Mathf.Abs(Vector3.Dot(right, agentToTree)) < 0.7f)
                {
                    // Determine steering direction
                    // If projection onto right is positive
                    if(Vector3.Dot(right, agentToTree) > 0)
                    {
                        // Turn left
                        Vector3 desiredVelocity = right * -1;

                        // Scale desired velocity
                        desiredVelocity.Normalize();
                        desiredVelocity *= maxSpeed;

                        // Rotate agent
                        transform.forward = desiredVelocity * Time.deltaTime;
                        
                        // Calculate the force 
                        Vector3 steeringForce = desiredVelocity - velocity;

                        // Apply steering force
                        ApplyForce(steeringForce);
                    }
                    // If projection onto right is negative
                    else
                    {
                        // Turn right
                        Vector3 desiredVelocity = right;

                        // Scale desired velocity
                        desiredVelocity.Normalize();
                        desiredVelocity *= maxSpeed;

                        // Rotate agent
                        transform.forward = desiredVelocity * Time.deltaTime;
                        
                        // Calculate the force 
                        Vector3 steeringForce = desiredVelocity - velocity;

                        // Apply steering force
                        ApplyForce(steeringForce);
                    }
                }
            }
        }
    }

    // Calculates wandering force ***************************************************************
    protected virtual void Wander ()
    {
        // Local Variables
        Vector3 center = transform.forward * 15;
        Vector3 radius = new Vector3(0, 0, 0.25f);
        Vector3 desiredVelocity = new Vector3();
        Vector3 wanderingForce = new Vector3();

        // Calculate wandering angle
        wanderingAngle += Random.Range(-10f, 10f);

        // Rotate radius
        radius = Quaternion.Euler(0, wanderingAngle, 0) * radius;

        // Calculate the desired velocity
        desiredVelocity = center + radius;

        // Scale desired velocity
        desiredVelocity.Normalize();
        desiredVelocity *= wanderingSpeed;

        // Rotate agent
        transform.forward = desiredVelocity * Time.deltaTime;

        // Calculate the force 
        wanderingForce = desiredVelocity - velocity;

        // Apply force
        ApplyForce(wanderingForce);

    }

    // Triggered when collision occurs **********************************************************
    protected virtual void OnCollision ()
    {

    }

    // Calculates the future position of agent **************************************************
    protected virtual void CalcFuturePos()
    {
        futurePosition = agentPosition + (velocity * 0.25f);
    }
}
