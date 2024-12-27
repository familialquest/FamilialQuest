using System;
using UnityEngine;

public class Item_QuestListSimple_Controller : MonoBehaviour
{
    public TaskStatusIconController TaskStatus;
    private TextFieldsFiller m_textFieldsFiller;
    public GroupElementController GroupElementController;
    // Start is called before the first frame update
    void Start()
    {
        try
        {
            m_textFieldsFiller = GetComponent<TextFieldsFiller>();

            TaskStatus.SetStatus(Code.Models.REST.CommonType.Tasks.Utils.StatusFromString(m_textFieldsFiller.TextData["Status"].ToString()), m_textFieldsFiller);

            if (GroupElementController != null)
            {
                //GroupElementController.SetVisible("Cost", !string.IsNullOrEmpty(m_textFieldsFiller.TextData["Cost"]));
                //GroupElementController.SetVisible("Penalty", !string.IsNullOrEmpty(m_textFieldsFiller.TextData["Penalty"]));
                //GroupElementController.SetVisible("AvailableUntil", !string.IsNullOrEmpty(m_textFieldsFiller.TextData["AvailableUntil"]));
                //GroupElementController.SetVisible("SolutionTime", !string.IsNullOrEmpty(m_textFieldsFiller.TextData["SolutionTime"]));
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
            throw;
        }
    }
}
