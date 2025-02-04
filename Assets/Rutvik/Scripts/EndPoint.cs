using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndPoint : MonoBehaviour
{
    public bool isPowerUp;
    public int colourID;
    public GameObject tunnel;
    public Material[] trainColorMaterials;

    public void PlayTunelAnimation()
    {
        if (tunnel != null)
            tunnel.GetComponent<Animation>().Play();
    }

    [ButtonInspector]
    public void TunnelColorSetup()
    {
        MeshRenderer renderer = tunnel.GetComponent<MeshRenderer>();
        renderer.material = trainColorMaterials[colourID];
    }
}
