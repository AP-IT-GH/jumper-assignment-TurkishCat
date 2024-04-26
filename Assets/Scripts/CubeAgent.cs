using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class CubeAgent : Agent
{
    public Transform Target;
    public GameObject redZone;
    public GameObject greenZone;
    public GameObject wholeFloor;

    public override void OnEpisodeBegin()
    {

        isTargetReached = false; // Reset de status van het bereiken van het target
        this.transform.position = RandomSpawn(greenZone);
        this.transform.rotation = Quaternion.identity;

        Target.position = RandomSpawn(redZone);
        Target.gameObject.SetActive(true);

    }


    Vector3 RandomSpawn(GameObject zone)
    {
        Bounds bounds = zone.GetComponent<Collider>().bounds;
        return new Vector3(
            Random.Range(bounds.min.x, bounds.max.x),
            0.5f,
            Random.Range(bounds.min.z, bounds.max.z)
        );
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(Target.position);
        sensor.AddObservation(this.transform.position);
    }

    public float speedMultiplier = 0.1f;
    bool isTargetReached = false;

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        Vector3 controlSignal = Vector3.zero;
        controlSignal.x = actionBuffers.ContinuousActions[0];
        controlSignal.z = actionBuffers.ContinuousActions[1];
        transform.Translate(controlSignal * speedMultiplier, Space.World);

        float distanceToTarget = Vector3.Distance(this.transform.position, Target.position);
        if (distanceToTarget < 1.42f && !isTargetReached)
        {
            SetReward(0.5f);
            isTargetReached = true;
            Target.gameObject.SetActive(false);
            Debug.Log("Target reached!");
        }

        CheckZone(greenZone, "Green");
        CheckZone(redZone, "Red");

        if (!checkFloor(wholeFloor))
        {
            EndEpisode();
        }

        if (this.transform.position.y < 0)
        {
            Debug.Log("Agent has fallen!");
            EndEpisode();
        }
    }

    private void CheckZone(GameObject zone, string zoneName)
    {
        if (zone.GetComponent<Collider>().bounds.Contains(this.transform.position))
        {
            Debug.Log($"In {zoneName} zone!");
            if (zoneName == "Green" && isTargetReached)
            {
                SetReward(1.0f);
                Debug.Log($"{zoneName} zone! Reward granted.");
                EndEpisode();
            }
        }

    }

    private bool checkFloor(GameObject floor)
    {
        Collider collider = floor.GetComponent<BoxCollider>();
        return collider.bounds.Contains(this.transform.position);
    }



    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Vertical");
        continuousActionsOut[1] = Input.GetAxis("Horizontal");
    }
}