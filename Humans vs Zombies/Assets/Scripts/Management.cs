using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Management : MonoBehaviour
{
    // Local variables
    public int startQuantityHumans;         // Number of humans at game start
    public int startQuantityZombies;        // Number of zombies at game start
    public bool zombiesWon = false;         // Win condition game state flag
    public bool debugMode = false;          // Debug state
    public float verticleOffset = 0.12f;    // Used to compensate for 3d anchor
    public float parkRadius = 4f;           // Spawn area radius
    public float agentRadius = 0.05f;       // Radius of humans and zombies
    public GameObject human;                // Human prefab
    public GameObject[] humans;             // Array of references to human instances
    public GameObject[] infected;           // Array of newly infected humans
    public GameObject zombie;               // Zombie prefab
    public GameObject[] zombies;            // Array of references to zombie instances
    public float lineOfSight;               // Distance the humans can see


    // Use this for initialization ****************************************************************************************************************
    void Start ()
    {
		// Instantiate humans
        for(int i = 0; i < startQuantityHumans; i++)
        {
            // Generate a random location in park
            Vector3 spawnPos = new Vector3(Random.Range(-parkRadius, parkRadius), verticleOffset, Random.Range(-parkRadius, parkRadius));

            // Generate a radom rotation about the Y axis
            Quaternion spawnRot = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

            // Instantiate instance of human prefab
            Instantiate(human, spawnPos, spawnRot);
        }

        // Instantiate zombies
        for (int i = 0; i < startQuantityZombies; i++)
        {
            // Generate a random location in park
            Vector3 spawnPos = new Vector3(Random.Range(-parkRadius, parkRadius), verticleOffset, Random.Range(-parkRadius, parkRadius));

            // Generate a radom rotation about the Y axis
            Quaternion spawnRot = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

            // Instantiate instance of human prefab
            Instantiate(zombie, spawnPos, spawnRot);
        }

        // Generate initial arrays
        GatherAgents();
    }
	
	// Update is called once per frame ************************************************************************************************************
	void Update ()
    {
        CheckForInput();

        // If humans still exist
        if (!zombiesWon)
        {
            // Find human closest to each zombie
            AcquireTargets();

            // Find closest to each human and check for collisions
            AcquirePredator();

            // Check for collisions
            CollisionManager();

            // Destroy newly infected humans
            BurnInfected();

            // Reset collections
            GatherAgents();
        }
	}

    // Check if buttons have been pressed *************************************************************************************************
    public void CheckForInput ()
    {
        // If 'D' is pressed
        if (Input.GetKeyDown(KeyCode.D))
        {
            // If not in debug mode
            if(!debugMode)
            {
                // Activate debug mode
                debugMode = true;
            }
            // If in debug mode
            else
            {
                //Deactivate debugMode
                debugMode = false;
            }
        }

        // If 'Z' is pressed
        if (Input.GetKeyDown(KeyCode.Z))
        {
            // Generate a random location in park
            Vector3 spawnPos = new Vector3(Random.Range(-parkRadius, parkRadius), verticleOffset, Random.Range(-parkRadius, parkRadius));

            // Generate a radom rotation about the Y axis
            Quaternion spawnRot = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

            // Instantiate instance of human prefab
            Instantiate(zombie, spawnPos, spawnRot);
        }

        // If 'R' is pressed
        if (Input.GetKeyDown(KeyCode.R))
        {
            // Reload the scene
            SceneManager.LoadScene(sceneName: "HvZ p04");
        }

        // If 'Esc' is pressed
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Exit application
            Application.Quit();
        }

    }

    // Finds the human closest to each zombie ***************************************************************************************************
    public void AcquireTargets ()
    {
        // Local variables
        GameObject closestHuman = null;
        float shortestDistance = 100f;

        // For each zombie
        for (int z = 0; z < zombies.Length; z++)
        {
            // Compare to each human
            for(int h = 0; h < humans.Length; h++ )
            {
                // Calculate the distince
                Vector3 zombieToHuman = humans[h].transform.position - zombies[z].transform.position;
                float distance = zombieToHuman.magnitude;

                // If this human is closer
                if(distance < shortestDistance)
                {
                    // Store human and distance
                    shortestDistance = distance;
                    closestHuman = humans[h];
                }
            }

            // Set each zombies target
            zombies[z].GetComponent<Zombie>().target = closestHuman;


            // Reset local variables
            closestHuman = null;
            shortestDistance = 100f;
        }
    }

    // Overloaded - Finds the human closest to passed zombie ***************************************************************
    public void AcquireTargets(GameObject newZombie)
    {
        // Local variables
        GameObject closestHuman = null;
        float shortestDistance = 100f;

        // Compare to each human
        for (int h = 0; h < humans.Length; h++)
        {
            // Calculate the distince
            Vector3 zombieToHuman = humans[h].transform.position - newZombie.transform.position;
            float distance = zombieToHuman.magnitude;

            // If this human is closer
            if (distance < shortestDistance)
            {
                // Store human and distance
                shortestDistance = distance;
                closestHuman = humans[h];
            }
        }

        // Pass the target to this zombie
        newZombie.GetComponent<Zombie>().target = closestHuman;

        // Reset local variables
        closestHuman = null;
        shortestDistance = 100f;
        
    }

    // Find zombie closest to each human ************************************************************************************
    public void AcquirePredator()
    {
        // Local variables
        GameObject closestZombie = null;
        float shortestDistance = 100f;

        // For each human
        for (int h = 0; h < humans.Length; h++)
        {
            // Compare to each zombie
            for (int z = 0; z < zombies.Length; z++)
            {
                // Calculate the distince
                Vector3 humanToZombie = zombies[z].transform.position - humans[h].transform.position;
                float distance = humanToZombie.magnitude;

                // If this human is closer
                if (distance < shortestDistance)
                {
                    // Store human and distance
                    shortestDistance = distance;
                    closestZombie = zombies[z];
                } 
            }

            // If the human is not already purged
            if (humans[h] != null)
            {
                // Pass the target to this zombie
                humans[h].GetComponent<Human>().predator = closestZombie;

                // If the shortest distance is greater than line of sight
                if (shortestDistance > lineOfSight)
                {
                    // Human needs to wander
                    humans[h].GetComponent<Human>().wandering = true;
                }
                // If closest zombie is in line of sight
                else
                {
                    // Human is no longer wandering
                    humans[h].GetComponent<Human>().wandering = false;
                }
            }

            // Reset local variables
            closestZombie = null;
            shortestDistance = 100f;
        }
    }

    // Checks each human to each zombie to check for infection *******************************************************************
    public void CollisionManager ()
    {
        // Local variables

        // For each human
        for (int h = 0; h < humans.Length; h++)
        {
            // Compare to each zombie
            for (int z = 0; z < zombies.Length; z++)
            {
                // Calculate the distince
                Vector3 humanToZombie = zombies[z].transform.position - humans[h].transform.position;
                float distance = humanToZombie.magnitude;

                // If this human is touching a zombie
                if (distance < agentRadius * 2)
                {
                    // Tag human as infected
                    humans[h].tag = "Infected";
                }
            }
            
        }

    }

    // Gathers and destroys infected humans ***************************************************************************************
    public void BurnInfected()
    {
        // Local variables
        Vector3[] infectedTranformData;

        // Collect infected
        infected = GameObject.FindGameObjectsWithTag("Infected");
        infectedTranformData = new Vector3[infected.Length];

        // For each infected human
        for ( int i = 0; i < infected.Length; i++)
        {
            // Store infected's transform data
            infectedTranformData[i] = infected[i].transform.position;

            // Destroy the infected human
            Destroy(infected[i]);
        }

        // For each human destroyed
        for (int t = 0; t < infectedTranformData.Length; t++)
        {
            // Spawn a zombie in said location
            Instantiate(zombie, infectedTranformData[t], Quaternion.identity);
        }

        // Reset the arrays
        infected = new GameObject[0];
        infectedTranformData = new Vector3[0];
    }

    // Gather agents *************************************************************************************************************
    public void GatherAgents()
    {
        // Create initial array of humans
        humans = GameObject.FindGameObjectsWithTag("Human");

        // Add all zombies in scene to a new array
        zombies = GameObject.FindGameObjectsWithTag("Zombie");

        // If theres no more humans
        if (humans.Length == 0)
        {
            // Set game state flag
            zombiesWon = true;

            // Zombies begin to wander
            for(int z = 0; z < zombies.Length; z++)
            {
                // Set zombies wander game state
                zombies[z].GetComponent<Zombie>().wandering = true;
            }

        }

        
    }
}
