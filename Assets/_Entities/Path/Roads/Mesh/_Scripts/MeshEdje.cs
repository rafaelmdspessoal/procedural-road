using Roads;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshEdje : MonoBehaviour
{
    public enum EdjePosition
    {
        StartCenter,
        StartLeft,
        StartRight,
        EndCenter,
        EndLeft,
        EndRight,
    }

    private EdjePosition edjePosition;

    public EdjePosition EdjePos => edjePosition;

    public void Init(EdjePosition edjePosition)
    {
        this.edjePosition = edjePosition;        
        transform.name = edjePosition.ToString();
    }
}
