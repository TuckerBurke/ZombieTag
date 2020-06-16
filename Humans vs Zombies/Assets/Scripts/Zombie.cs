using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zombie : Agent
{
    // Zombie specific variables
    public Material zombieToHuman;      // Distance to nearest human
    public Material zombieFuturePos;    // To implement in p-04
    public GameObject target;           // Closest human
	
	// Update is called once per frame ***********************************************************
	void Update ()
    {
        // Stay in bounds
        Boundaries();

        // Obtain a reference to array of all zombies
        GameObject[] allZombies = sceneManager.GetComponent<Management>().zombies;

        

        // If the zombie is not wandering
        if (!wandering)
        {
            // Seek closest human
            //Seek(target);

            // Pursue closest human
            Pursue(target);

            // Apply forces and move
            Move();
        }
        
        // While the zombie is wandering
        else
        {
            // Wander (P-04)
            Wander();
            Move();
        }

        // Separation between zombies
        Separate(allZombies);

        // Avoid Obsticles
        Avoid();

        // Calculate agents new future position (1/4 second out)
        CalcFuturePos();
	}

    // Additional debug lines ****************************************************************
    protected override void OnRenderObject()
    {
        // If the scene is in debug mode
        if (sceneManager != null && sceneManager.GetComponent<Management>().debugMode)
        {
            // Draw lines from base class
            base.OnRenderObject();

            // If a target has been acquired
            if (target != null)
            {
                // Draw line from zombie to closest human
                zombieToHuman.SetPass(0);
                GL.Begin(GL.LINES);
                GL.Vertex(transform.position);
                GL.Vertex(target.transform.position);
                GL.End();
            }

            //Draw line for zombie's future position
            zombieFuturePos.SetPass(0);
            GL.Begin(GL.LINES);
            GL.Vertex(transform.position);
            GL.Vertex(futurePosition);
            GL.End();
        }
    }

    // Calculates seeking force ****************************************************************************************
    public void Seek(Vector3 targetPosition)
    {
        // Calculate desired velocity
        Vector3 desiredVelocity = targetPosition - agentPosition;

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

    // Overloaded seek that takes game object **************************************************************************
    public void Seek(GameObject closestHuman)
    {
        // Call seek passing target position
        Seek(closestHuman.transform.position);
    }

    // Calculates pursue force ***************************************************************************************
    public void Pursue(Vector3 targetFuturePosition)
    {
        // Calculate desired velocity
        Vector3 desiredVelocity = targetFuturePosition - agentPosition;

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

    // Overloaded pursue that takes game object **************************************************************************
    public void Pursue(GameObject closestHuman)
    {
        // Call pursue passing target position
        Seek(closestHuman.GetComponent<Human>().futurePosition);
    }
}
