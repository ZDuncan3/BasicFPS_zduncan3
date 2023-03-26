using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchRotation : MonoBehaviour
{
    public GameObject objectToRotateWith;
    public GameObject objectToMoveWith;

    void LateUpdate()
    {
        if (objectToRotateWith != null)
            this.gameObject.transform.rotation = objectToRotateWith.transform.rotation;

        if (objectToMoveWith != null)
            this.gameObject.transform.position = objectToMoveWith.transform.position;
    }
}
