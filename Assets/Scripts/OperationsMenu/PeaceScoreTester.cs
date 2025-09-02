using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PeaceScoreTester : MonoBehaviour
{
    public TMP_Text lbl_heart;
    public TMP_Text lbl_minds;
    public TMP_Text lbl_peace;
    public float opDay = 1;
    public float opDuration = 1;
    public float hWeight = 1.0f;
    public float mWeight = 1.0f;
    public float dWeight = 1.2f;

    // Update is called once per frame
    void Update()
    {
        int hearts = int.Parse(lbl_heart.text);
        int minds = int.Parse(lbl_minds.text);

        //int peacescore = (int)Mathf.Clamp(((Mathf.Log(hearts + minds) - Mathf.Log(((100f-(hearts * .75f)) + (100f-minds)))) * 100f) - ((opDay/opDuration) * 20f), 0, 100);
        int peacescore = (int)Mathf.Clamp((((hearts * hWeight) + (minds * mWeight)) / 2) - ((opDay / opDuration) * dWeight), 0, 100);

        lbl_peace.text = peacescore.ToString();
    }
}
