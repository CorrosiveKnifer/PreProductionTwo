using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDManager : MonoBehaviour
{
    #region Singleton

    public static HUDManager instance = null;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.LogError("Second Instance of HUDManager was created, this instance was destroyed.");
            Destroy(this);
        }
    }

    private void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }

    #endregion

    [Header("UI Objects")]
    public GameObject[] m_UIElements;

    private void Start()
    {
        gameObject.name = $"HUDManager ({gameObject.name})";
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 mousePos = InputManager.instance.GetMousePositionInScreen();
        if (InputManager.instance.GetMouseButtonDown(MouseButton.LEFT))
        {
            foreach (var elements in m_UIElements)
            {
                if(elements.GetComponent<UI_Element>() != null 
                    && elements.GetComponent<UI_Element>().IsContainingVector(mousePos))
                {
                    elements.GetComponent<UI_Element>().OnMouseDownEvent();
                }
            }
        }
        else if(InputManager.instance.GetMouseButtonUp(MouseButton.LEFT))
        {
            foreach (var elements in m_UIElements)
            {
                 elements.GetComponent<UI_Element>().OnMouseUpEvent();
            }
        }
    }

    public UI_Element GetElementUnderMouse()
    {
        Vector2 mousePos = InputManager.instance.GetMousePositionInScreen();
        foreach (var elements in m_UIElements)
        {
            if (elements.GetComponent<UI_Element>() != null
                && elements.GetComponent<UI_Element>().IsContainingVector(mousePos))
            {
                return elements.GetComponent<UI_Element>();
            }
        }
        return null;
    }

    public UI_Element GetElementByType(System.Type type)
    {
        foreach (var item in m_UIElements)
        {
            if(item.GetComponent<UI_Element>().GetType() == type)
            {
                return item.GetComponent<UI_Element>();
            }
        }
        return null;
    }
}
