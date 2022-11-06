using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bridge2 : MonoBehaviour
{
    public GameObject bridgeElement, BridgePlanks, BridgeJointStart, BridgeJointEnd;
    public Transform BridgeStart, BridgeEnd;

    private float bridgeDistace;
    public int inacuracies;
    private GameObject prevPart;

    private Vector3 startPos;

    // Start is called before the first frame update
    void Start()
    {
        // Getting length of the bridge
        startPos = BridgeStart.position;
        bridgeDistace = Vector3.Distance(startPos, BridgeEnd.position);
        // Getting Direction of the bridge
        //Vector3 Direction = startPos - BridgeEnd.position;
        BridgeStart.LookAt(BridgeEnd, BridgeStart.up);
        BridgeEnd.LookAt(BridgeStart, BridgeEnd.up);


        //prevPart = bridgeElement;
        //finds how many extra planks should be added from decimal range
        // have a int that can be used to fix inacuracies
        int LeftoverPlanks = (int)(((bridgeDistace* 2.7f)%1)/3*10); // 2.7f is a found value on how many planks we want per meeter.
        int PlankCount = (int)(bridgeDistace* 2.7f) + LeftoverPlanks + inacuracies; //~3 planks per meeter 
        // Debug.Log(PlankCount + " - " + LeftoverPlanks + " - " + bridgeDistace* 2.7f);
        // Debug.Log(Vector3.Distance(BridgeStart.position, BridgeEnd.position));

        for (int i = 0; i <= PlankCount; i++)
        {
            GameObject instantiatedPlank = Instantiate(bridgeElement);
            instantiatedPlank.transform.SetParent(BridgePlanks.transform, false);
            instantiatedPlank.transform.localPosition = new Vector3(0, 0, i * .35F);
                //instantiatedPlank.GetComponent<Rigidbody>().isKinematic = true;

            if (i == 0)
            {
                instantiatedPlank.GetComponent<HingeJoint>().connectedBody = BridgeJointStart.GetComponent<Rigidbody>();
            }
            if (i != 0) {

                instantiatedPlank.GetComponent<HingeJoint>().connectedBody = prevPart.GetComponent<Rigidbody>();
                //Debug.Log(instantiatedPlank.transform.position);
                //Debug.Log(i + " - " + bridgeDistace);
                //Debug.Log(i);
            }
            if (i == PlankCount)
            {
                // instantiatedPlank.GetComponent<HingeJoint>().connectedBody = prevPart.GetComponent<Rigidbody>();
                // BridgeJointEnd.GetComponent<HingeJoint>().connectedBody = instantiatedPlank.GetComponent<Rigidbody>();
            }
            
            prevPart = instantiatedPlank;
        }
    }
}
