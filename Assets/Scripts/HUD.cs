using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HUD : MonoBehaviour
{
    private RectTransform m_WebIcon;

    // Start is called before the first frame update
    void Start()
    {
        m_WebIcon = transform.Find("WebIcon").GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
