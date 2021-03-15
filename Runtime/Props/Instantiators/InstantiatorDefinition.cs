using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstantiatorDefinition : MonoBehaviour
{
    public enum TextSize
    {
        Small,
        Medium,
        Big
    };

    public TextSize PropTextSize = TextSize.Small;

    public enum TextColor
    {
        Red,
        White,
        Blue,
        Yellow,
        Pink,
        Green
    };

    public TextColor PropTextColor = TextColor.Red;
}
