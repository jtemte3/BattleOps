using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ObjectiveManager : MonoBehaviour
{
    public List<ObjectiveEvent> objectiveList = new();
    public TMP_Text objectiveUIText;
    // Start is called before the first frame update
    void Start()
    {
        foreach (ObjectiveEvent obj in objectiveList)
        {
            obj.DeactivateObjective();
        }

        objectiveList[0].ActivateObjective();
    }

    // Update is called once per frame
    void Update()
    {
        foreach (ObjectiveEvent obj in objectiveList)
        {
            if (obj.isObjActive == true && obj.isObjCompleted == false)
            {
                obj.Engage();

                if (objectiveUIText != null)
                {
                    objectiveUIText.text = obj.objectiveShortDescription;
                }
            }
        }
    }
}
