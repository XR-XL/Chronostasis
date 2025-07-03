using UnityEngine;
using UnityEngine.UI;

public class DashBar : MonoBehaviour
{
    public Image dashSliderOne;
    public Image dashSliderTwo;

    // fill is [0,1]

    public void SetDashOne(float dashValue)
    {
        dashSliderOne.fillAmount = dashValue;
    }

    public void SetDashTwo(float dashValue) 
    {
        dashSliderTwo.fillAmount = dashValue;
    }


}
