using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DebugLoadTime : UIElement
{
    public TMP_Text loadLabel;
    public GridVoronoiCity cityParent;
    private string loadLabelFormat;
    // Start is called before the first frame update
    void Start()
    {
        loadLabelFormat = loadLabel.text;
    }

    public override void Activate()
    {
        loadLabel.text = string.Format(loadLabelFormat, cityParent.elapsedTime);
    }
}
