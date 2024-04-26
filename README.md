# Welcome to the Jumper Assignment of Emirhan Ramazan Sahin

In this assignment we are training a cube to jump over a wall and stay still to catch a target. 
The most important thing to know is that this exercise could've been done in many different ways. I had decided to do it using a Ray Perception Sensor 3D component for object recognition.
In the end the cube was able to decide whether to jump or not based on the generated item.


# Used components and objects

The most important components for the exercise were:

 1. Ray Perception Sensor 3D & Behaviour Parameters
 2. Script having logic for observations and actions etc..
 3. Items to recognize


## Ray Perception Sensor 3D & Behaviour Parameters

This component of the ML Agent Cube has the purpose of detecting objects in iets view(the rays). I configured it so that it could detect items with the tags Wall and Target. 
The wall being the wall the Agent had to dodge in the assignment.

![Sensor Config](https://i.imgur.com/fq1yC4u.png)

The behaviour parameters can be seen in the image. Below the image you can see the yaml file config used during the training.

![Behaviour Parameters](https://i.imgur.com/n3c4iL8.png)

     behaviors:
     CubeAgentRays:
     trainer_type: ppo
     hyperparameters:
      batch_size: 10
      buffer_size: 100
      learning_rate: 3.0e-4
      beta: 5.0e-4
      epsilon: 0.2
      lambd: 0.99
      num_epoch: 3
      learning_rate_schedule: linear
      beta_schedule: constant
      epsilon_schedule: linear
     network_settings:
      normalize: false
      hidden_units: 128
      num_layers: 2
     reward_signals:
      extrinsic:
        gamma: 0.99
        strength: 1.0
     max_steps: 125000
     time_horizon: 64
     summary_freq: 2000

   
## Script for ML Agent

    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using Unity.MLAgents;
    using Unity.MLAgents.Actuators;
    using Unity.MLAgents.Sensors;
    using UnityEngine;
    
    
    public class CubeAgentRays : Agent
    {
        public GameObject ResetCube;
        public GameObject ObjectToDodge;
        public GameObject ObjectToCatch;
        public GameObject GeneratedItem;
        public GameObject SpawnPoint;
        int itemNumber;
        float moveSpeed;
        public override void OnEpisodeBegin()
        {
            // reset de positie en orientatie als de agent gevallen is
    
            this.transform.localPosition = new Vector3(12f, 0.5f, 0);
            ResetCube.transform.localPosition = new Vector3(20f, 0.5f, 0);
            itemNumber = Random.Range(1, 5);
    
            if (GeneratedItem != null)
            {
                Destroy(GeneratedItem);
            }
            moveSpeed = Random.Range(1f, 2f);
            if (itemNumber < 4)
            {
                GeneratedItem = Instantiate(ObjectToDodge, new Vector3(SpawnPoint.transform.localPosition.x, SpawnPoint.transform.localPosition.y, SpawnPoint.transform.localPosition.z), Quaternion.identity);
            }
            else
            {
                GeneratedItem = Instantiate(ObjectToCatch, new Vector3(SpawnPoint.transform.localPosition.x, SpawnPoint.transform.localPosition.y, SpawnPoint.transform.localPosition.z), Quaternion.identity);
            }
    
        }
        void Start()
        {
            Rigidbody rb = GetComponent<Rigidbody>();
            // Freeze de X en Z posities
            rb.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
        }
    
        void Update()
        {
            if (GeneratedItem != null)
            {
                float step = moveSpeed * Time.deltaTime; // Bereken hoe ver het object deze frame moet bewegen
                GeneratedItem.transform.localPosition = Vector3.MoveTowards(GeneratedItem.transform.localPosition, new Vector3(ResetCube.transform.localPosition.x, ResetCube.transform.localPosition.y, ResetCube.transform.localPosition.z), step);
            }
        }
    
        public override void CollectObservations(VectorSensor sensor)
        {
            // Target en Agent posities
            sensor.AddObservation(GeneratedItem);
            sensor.AddObservation(this.transform.localPosition);
        }
    
        public float jumpForce = 25f;
        private bool isGrounded = true;
        public override void OnActionReceived(ActionBuffers actionBuffers)
        {
            // Controleer of de agent van het platform is gevallen
            if (this.transform.localPosition.y < 0 || ResetCube.transform.localPosition.y < 0)
            {
                EndEpisode();
                return;
            }
    
            // Actie om te springen
            float jumpAction = actionBuffers.ContinuousActions[0];  // We gaan ervan uit dat dit de jump action is
            if (jumpAction > 0.5 && isGrounded)  // Drempelwaarde om te beslissen om te springen
            {
                GetComponent<Rigidbody>().AddForce(new Vector3(0, jumpForce, 0), ForceMode.Impulse);
    
                isGrounded = false;
            }
        }
    
        void OnCollisionEnter(Collision collision)
        {
            if (collision.collider.tag.Contains("Floor"))
            {
                isGrounded = true;
                Debug.Log("Ik sta op vloer!");
    
            }
            if (collision.collider.tag.Contains("Target"))
            {
                Debug.Log("Taget hit the main cube");
                SetReward(1f);
                EndEpisode();
            }
            if (collision.collider.tag.Contains("Wall"))
            {
                Debug.Log("Wall hit the main cube");
                SetReward(-1f);
                EndEpisode();
            }
    
    
        }
    
        public override void Heuristic(in ActionBuffers actionsOut)
        {
            var continuousActionsOut = actionsOut.ContinuousActions;
            continuousActionsOut[0] = Input.GetAxis("Vertical");
        }
    
    
        public void Wall_Passed()
        {
            SetReward(1f);
            EndEpisode();
        }
    
        public void Target_Passed()
        {
            SetReward(-1f);
            EndEpisode();
        }
    
    
    }
    
 This script should be pretty self-explanatory. The methods Wall_Passed and Target_Passed are being called in the ResetCube object script to detect if something has collided with it. Look below:
 

    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    
    public class ResetCube : MonoBehaviour
    {
        public CubeAgentRays MainCube;
    
        private void OnCollisionEnter(Collision collision)
        {
            if (collision.collider.tag == "Wall")
            {
                Debug.Log("Wall passed the main cube");
                MainCube.Wall_Passed();
            }
    
    
    
            else if (collision.collider.tag == "Target")
            {
                Debug.Log("Target passed the main cube");
                MainCube.Target_Passed();
            }
    
        }
    }

![Script items attached from Hierarchy](https://i.imgur.com/wIZvenp.png)
## Items to recognize
The items to recognize are simple prefabs of a Wall and a Ball which is the target. You can see in the script of the ML Agent we need to assign the prefabs to this script via the UI so it knows the prefab to spawn of a spawner on the map.


## Optimizing the training 

During the training some challenges occured. At some points my GPUless slow laptop just started lagging alot, making the training sub optimal. I made my ray lengths very long because my Agent had to detect what was coming from a far. Making it start its jumping cycle very early. Sometimes resulting in crashing ontop of the wall, even though it's intentions were to dodge it.

![Ray length](https://i.imgur.com/dvJ7EHf.png)
I sadly couldn't shorten my ray lengths because when the training was going on for quite a while my laptop would start to slow down, not giving the game engine enough time to decide whether to jump over the wall or not. So I had to keep my ray lenghts long so the agent always knew what was coming towards it.

Here you can see what kind of effects your laptop slowing down has on your training:
![Big crash due to laptop slowing down](https://i.imgur.com/kmcmULU.png)
You can clearly see the big crash after having trained succesfully for quite a while.
Below another example of having big crashes when laptop was slowing down:
![Another training scenario](https://i.imgur.com/D06HFls.png)

As you can see in both training scenario's the negative effects start to occur after 100 000 steps.


## Video
[Video of training the agent](https://ap.cloud.panopto.eu/Panopto/Pages/Viewer.aspx?id=4ca9b8e7-47f3-44f4-a777-b15e016be1e8)

In the provided video you can clearly see as the training goes, the agent progessively decides better whether or not to jump when seeing a target that results in positive or negative reward points. 
Colliding with the ball/target is positive reward points. Colliding with the wall is negative reward points.
Jumping over the ball/target is negative reward points. Jumping over the wall is positive reward points.

In the video you can also visually see what I explained under 

> Optimizing the training

You can see the Agent jumping continuously when detecting a wall that it has to dodge.


## Conclusion
When training an agent, using the right training parameters is very important, aswell as setting up the right environment. I had completely different outcomes when I made the approaching item faster because the Agent also needs time to process what's coming. Combining the right parameters with a well set up environment is the key to a well trained Agent Model.
