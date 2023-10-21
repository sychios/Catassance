using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class NodesManager : MonoBehaviour
{
    private List<Transform> childrenTransform;

    // Add NodeUtilities Script to every node. Right now this is the only fix to the bug that prohibits roads being placed appending to this node 
    // TODO: Further research bug explained above
    void Awake()
    {
        childrenTransform = transform.GetComponentsInChildren<Transform>().ToList();
        childrenTransform.RemoveAt(0); // Remove parent, which is always at index 0
        childrenTransform.ForEach(child => child.AddComponent<NodeUtilities>());
    }
}
