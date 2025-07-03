using UnityEngine;
using UnityEngine.UI;

public class TimestopBar : MonoBehaviour
{
    public Image timestopBar;

    public void SetTimestopBar(float Value)
    {
        timestopBar.fillAmount = Value;
    }

}
