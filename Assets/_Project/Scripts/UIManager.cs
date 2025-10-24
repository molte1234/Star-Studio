using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("Stat Displays")]
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI fansText;
    public TextMeshProUGUI yearText;
    public TextMeshProUGUI quarterText;
    public TextMeshProUGUI unityText;

    [Header("Action Buttons")]
    public Button recordButton;
    public Button tourButton;
    public Button practiceButton;
    public Button restButton;
    public Button releaseButton;

    [Header("Event Popup")]
    public GameObject eventPopupPanel;
    public TextMeshProUGUI eventTitleText;
    public TextMeshProUGUI eventDescriptionText;
    public Image eventImage;
    public Button[] choiceButtons; // 2-3 buttons

    void Start()
    {
        // Wire up action buttons
        recordButton.onClick.AddListener(() => OnActionClicked(ActionType.Record));
        tourButton.onClick.AddListener(() => OnActionClicked(ActionType.Tour));
        practiceButton.onClick.AddListener(() => OnActionClicked(ActionType.Practice));
        restButton.onClick.AddListener(() => OnActionClicked(ActionType.Rest));
        releaseButton.onClick.AddListener(() => OnActionClicked(ActionType.Release));

        eventPopupPanel.SetActive(false);
        UpdateUI();
    }

    public void UpdateUI()
    {
        // Why: Refresh all stat displays with current game state
        GameManager gm = GameManager.Instance;

        moneyText.text = "$" + gm.money;
        fansText.text = gm.fans.ToString();
        yearText.text = "Year " + gm.currentYear;
        quarterText.text = "Quarter " + ((gm.currentQuarter % 4) + 1);
        unityText.text = "Unity: " + gm.unity;
    }

    private void OnActionClicked(ActionType actionType)
    {
        // Why: Player clicked an action, tell GameManager
        GameManager.Instance.DoAction(actionType);
    }

    public void ShowEvent(EventData evt)
    {
        // Why: Display event popup with choices
        eventPopupPanel.SetActive(true);
        eventTitleText.text = evt.eventTitle;
        eventDescriptionText.text = evt.eventDescription;
        eventImage.sprite = evt.eventSprite;

        // Set up choice buttons
        for (int i = 0; i < choiceButtons.Length; i++)
        {
            if (i < evt.choices.Length)
            {
                choiceButtons[i].gameObject.SetActive(true);
                int choiceIndex = i; // Capture for lambda
                choiceButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = evt.choices[i].choiceText;
                choiceButtons[i].onClick.RemoveAllListeners();
                choiceButtons[i].onClick.AddListener(() => OnChoiceClicked(choiceIndex));
            }
            else
            {
                choiceButtons[i].gameObject.SetActive(false);
            }
        }
    }

    public void HideEvent()
    {
        // Why: Close event popup
        eventPopupPanel.SetActive(false);
    }

    private void OnChoiceClicked(int choiceIndex)
    {
        // Why: Player picked a choice, tell EventManager
        GameManager.Instance.eventManager.PlayerChoseOption(choiceIndex);
    }
}