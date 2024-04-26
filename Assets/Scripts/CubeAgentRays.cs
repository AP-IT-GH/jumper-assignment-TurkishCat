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
        // Target en Agent posities
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
