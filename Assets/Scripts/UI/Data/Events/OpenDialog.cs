using Client.UI;
using NaughtyAttributes;
using System;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "OpenDialog",
menuName = "Scriptable Objects/UI/Event/OpenDialog", order = 1)]
public class OpenDialog : UIEvent
{
    [Flags]
    public enum EShowButtons
    {
        None = 0,
        Okay = 2,
        Cancel = 4,
        Submit = 8
    }

    private enum EDialogSize
    {
        Small = 0,
        Medium = 1,
        Large = 2,
    }

    [Header("Header/Title")]
    [SerializeField]
    private bool m_showHeader = true;
    [SerializeField]
    private string m_header;

    [Header("Body")]
    [SerializeField, ResizableTextArea]
    [InfoBox("Can be rich text")]
    private string m_bodyText;

    [Header("Display")]
    [SerializeField]
    private EDialogSize m_dialogSize;
    [SerializeField]
    private EShowButtons m_whatButtonsToShow = EShowButtons.Okay;

    [Header("Commands")]
    [SerializeField]
    private UICommand m_cancelBtnCmd;
    [SerializeField]
    private UICommand m_okBtnCmd;

    [Space]
    public GameObject testing;
    public override void Execute()
    {
        var test = Instantiate(testing);

        UIDialog diag = test.GetComponent<UIDialog>();

        diag.SetUpText(m_showHeader, m_header, m_bodyText);
        diag.SetUpButtons(m_whatButtonsToShow);

        //clean up & add dialog to command because SO saves data across session
        m_cancelBtnCmd.CommandList.First().EventList.Clear();
        m_cancelBtnCmd.CommandList[0].EventList.Add(diag);
        diag.CancelBtn.Events = m_cancelBtnCmd;
        
        diag.OkBtn.Events = m_okBtnCmd;

        diag.ShowDialog();
    }
}