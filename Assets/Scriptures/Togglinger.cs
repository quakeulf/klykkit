using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Togglinger : MonoBehaviour
{
    private MeshRenderer _renderer;
    private MeshRenderer _childRenderer;

    private static readonly string _toggleString = "Togglingers";

    bool _toggled = false;

    void OnEnable()
    {
        _childRenderer = transform.GetChild(0).GetComponent<MeshRenderer>();
        _renderer = GetComponent<MeshRenderer>();
        InvokeRepeating(_toggleString, Random.Range(0f, 0.25f), Random.Range(0.5f, 0.75f));
    }

    void OnDisable()
    {
        CancelInvoke();
    }

    void Togglingers()
    {
        _renderer.enabled = _childRenderer.enabled = _toggled;
        _toggled = !_toggled;
    }
}
