using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
// Michael Jordan
public class UI_CurveBar : UI_Element
{
    [Header("Child Images")]
    public Image m_curveSection;
    public Image m_curveBorderSection;
    public Image m_barSection;
    public Image m_barBorderSection;

    [Header("Scalers")]
    [Tooltip("Used to calculate the amount of width left for the remaining max HP after the curve is calculated.")]
    public float m_pixelsPerHP;
    [Tooltip("Will determine how much hp is reserved by the curve.")]
    public float m_ringFillPerHP;

    [Header("Curve fill restrictions")]
    [Tooltip("Because the curve isn't closed, you will have to predefine when the cut off for the fill is.")]
    public float m_curveMinVal;
    private float m_curveMaxVal;

    [Header("Player Stats")]
    public float m_maxHealth;
    [Range(0.0f, 1.0f)]
    public float m_currentHealth;

    private float m_curveShare;
    private float m_curvedHealthTotal;
    private float m_barHealthTotal;
    private Vector2 m_barSizeVector;

    // Update is called once per frame
    void Update()
    {        
        m_curvedHealthTotal = (1.0f - m_curveMinVal) / m_ringFillPerHP;

        m_barHealthTotal = m_maxHealth - m_curvedHealthTotal;
        m_barSizeVector = m_barSection.rectTransform.sizeDelta;
        m_barSection.rectTransform.sizeDelta = m_barSizeVector;

        if (m_barHealthTotal >= 0)
        {
            m_barSizeVector.x = m_barHealthTotal / m_pixelsPerHP;
            m_curveMaxVal = 1.0f;
            m_curveShare = m_curvedHealthTotal / m_maxHealth;
        }
        else
        {
            m_barSizeVector.x = 0;
            m_curveMaxVal = m_maxHealth / m_curvedHealthTotal;
            m_curveShare = 1.0f;
        }

        if (m_currentHealth >= m_curveShare)
        {
            //Adjust the bar only;
            float fill = ConvertValueToNewRange(m_currentHealth, 1.0f, m_curveShare, 1.0f, 0.0f);
            m_barSection.fillAmount = fill;
            m_curveSection.fillAmount = m_curveMaxVal;
        }
        else
        {
            float fill = ConvertValueToNewRange(m_currentHealth, m_curveShare, 0.0f, m_curveMaxVal, m_curveMinVal);

            m_barSection.fillAmount = 0.0f;
            m_curveSection.fillAmount = Mathf.Clamp(fill, m_curveMinVal, m_curveMaxVal);
        }

        m_curveBorderSection.fillAmount = m_curveMaxVal;

        m_barSection.rectTransform.sizeDelta = m_barSizeVector;
        m_barBorderSection.rectTransform.sizeDelta = m_barSizeVector;
    }

    //Converts from one range to another range and returns the value within the new range.
    private float ConvertValueToNewRange(float oldVal, float oldMax, float oldMin, float newMax, float newMin)
    {
        return (((oldVal - oldMin) * (newMax - newMin)) / (oldMax - oldMin)) + newMin;
    }


    public override bool IsContainingVector(Vector2 _pos)
    {
        //Do Nothing
        return false;
    }

    public override void OnMouseDownEvent()
    {
        //Do Nothing
        return;
    }

    public override void OnMouseUpEvent()
    {
        //Do Nothing
        return;
    }

}
