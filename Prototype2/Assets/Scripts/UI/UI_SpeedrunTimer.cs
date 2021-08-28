using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UI_SpeedrunTimer : UI_Element
{
    public TimeSpan timeElapsed { get; private set; }

    private float fastestTime;
    private DateTime startTime;
    private TextMeshProUGUI m_display;
    private bool timerRunning = false;
    private bool show = false;
    public void Start()
    {
        m_display = GetComponent<TextMeshProUGUI>();
        m_display.enabled = false;
    }
    // Start is called before the first frame update
    public void StartTimer()
    {
        timerRunning = true;
        show = true;
        startTime = DateTime.Now;
    }

    // Update is called once per frame
    void Update()
    {
        if(timerRunning)
            timeElapsed = DateTime.Now - startTime;

        if(m_display != null)
        {
            m_display.enabled = GameManager.instance.enableTimer && show;
            m_display.text = timeElapsed.ToString("g").Substring(0, 11);
        }
    }
    public void StopTimer()
    {
        timerRunning = false;
    }

    public override bool IsContainingVector(Vector2 _pos)
    {
        return false;
    }

    public override void OnMouseDownEvent()
    {
        //Do nothing
    }

    public override void OnMouseUpEvent()
    {
        //Do nothing
    }
}
