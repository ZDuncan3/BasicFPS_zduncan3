using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIMatchCameraRotation : MonoBehaviour
{
    public PlayerController player;

    private void Start()
    {

    }

    void LateUpdate()
    {
        if (!player.tacticalCamera)
            this.gameObject.transform.rotation = player.mainCamera.transform.rotation;
        else if (player.tacticalCamera)
            this.gameObject.transform.rotation = player.tacCamera.transform.rotation;
    }
}
