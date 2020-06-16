using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Human : Agent
{
    // Human specific variables
    public Material humanFuturePos;                     // To implement in p-04
    public GameObject predator;                         // The zombie closest zombie
    public GameObject[] predators = new GameObject[0];  // Array of near by zombies
	
	// Update is called once per frame ***************************************************************
	void Update ()
    {
        // Stay in bounds
        Boundaries();

        // Obtain a reference to array of all humans
        GameObject[] allHumans = sceneManager.GetComponent<Management>().humans;

        

        // If the human is not wandering
        if (!wandering)
        {
            // Flee closest zombie
            //Flee(predator);

            // Evade closest zombies
            Evade(predator);

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

        // Separation between like objects
        Separate(allHumans);

        // Avoid Obsticles
        Avoid();

        // Calculate new future position (1/4 sec out)
        CalcFuturePos();

    }

    // Additional debug lines *************************************************************************
    protected override void OnRenderObject()
    {
        if (sceneManager != null && sceneManager.GetComponent<Management>().debugMode)
        {
            // Draw lines from base class
            base.OnRenderObject();

            // Draw line for humans future position
            humanFuturePos.SetPass(0);
            GL.Begin(GL.LINES);
            GL.Vertex(transform.position);
            GL.Vertex(futurePosition);
            GL.End();
        }
    }

    // Calculates flee force ****************************************************************************************
    public void Flee(Vector3 predatorPosition)
    {
        // Calculate desired velocity
        Vector3 desiredVelocity = agentPosition - predatorPosition;

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

    // Overloaded flee that takes game object ***********************************************************************
    public void Flee(GameObject closestZombie)
    {
        // Call seek passing target position
        Flee(closestZombie.transform.position);
    }

    // Calculates evasion force ****************************************************************************************
    public void Evade(Vector3 predatorFuturePosition)
    {
        // Calculate desired velocity
        Vector3 desiredVelocity = agentPosition - predatorFuturePosition;

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

    // Overloaded evasion that takes array of game objects ***********************************************************************
    public void Evade(GameObject closestZombie)
    {
        // Call evade passing targets future position
        Evade(closestZombie.GetComponent<Zombie>().futurePosition);
    }

}
