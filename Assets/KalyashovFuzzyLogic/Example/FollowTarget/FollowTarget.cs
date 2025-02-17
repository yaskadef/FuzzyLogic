using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FuzzyLogicSystem;
using Codice.CM.Common;

//Kalyashov, Mezhenin AUV control system based on fuzzy logic
public class FollowTarget : MonoBehaviour
{
    public Transform cubeTarget = null;

    public Transform AUV = null;



    public float reactionSpeed = 2.0f;



    float saveCurrentY = 0.0f;
    float offsetY = 0;

    public TextAsset fuzzyLogicData = null;
    private FuzzyLogic fuzzyLogic = null;

    private void Start()
    {
        fuzzyLogic = FuzzyLogic.Deserialize(fuzzyLogicData.bytes, null);

        saveCurrentY = transform.position.y;
        offsetY = GetTerrainDistance();
    }

    private void Update()
    {
        fuzzyLogic.evaluate = true;
        fuzzyLogic.GetFuzzificationByName("distance").value = 
            Vector3.Distance(cubeTarget.position, AUV.position);

        float speed = fuzzyLogic.Output() * fuzzyLogic.defuzzification.maxValue;
        Vector3 newPos = Maneuvering(speed);

        AUV.position = new Vector3(newPos.x, Mathf.Lerp(saveCurrentY, 
            Terrain.activeTerrain.SampleHeight(AUV.position) + offsetY, reactionSpeed * Time.deltaTime), 
            newPos.z);
        saveCurrentY = AUV.position.y;
    }

    Vector3 Maneuvering(float speed)
    {
        return Vector3.MoveTowards(AUV.position, cubeTarget.position, speed * Time.deltaTime);
    }

    float GetTerrainDistance()
    {
        RaycastHit hit;
        Physics.Raycast(transform.position, Vector3.down, out hit);

        return hit.distance;
    }
}
