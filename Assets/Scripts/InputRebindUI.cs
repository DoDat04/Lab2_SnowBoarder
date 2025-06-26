using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class InputRebindUI : MonoBehaviour
{
    [Header("Binding Info")]
    public string actionName;
    public int bindingIndex = 0;

    [Header("UI Elements")]
    public TextMeshProUGUI actionLabel;
    public TextMeshProUGUI bindingText;
    public Button rebindButton;
    public Button resetButton;

    private InputAction action;

    void Start()
    {
        var inputActions = UserInput.Instance.playerInput;
        action = inputActions.FindAction(actionName);

        if (action == null)
        {
            Debug.LogError($"❌ Action '{actionName}' not found.");
            return;
        }

        UpdateBindingDisplay();

        if (rebindButton != null)
            rebindButton.onClick.AddListener(StartRebinding);

        if (resetButton != null)
            resetButton.onClick.AddListener(ResetBindingOverride);

        LoadBindingOverride(); // Load khi start
    }

    public void UpdateBindingDisplay()
    {
        if (bindingText != null && action != null)
        {
            bindingText.text = InputControlPath.ToHumanReadableString(
                action.bindings[bindingIndex].effectivePath,
                InputControlPath.HumanReadableStringOptions.OmitDevice);
        }
    }

    public void StartRebinding()
    {
        if (action == null || bindingIndex >= action.bindings.Count)
        {
            Debug.LogError("❌ Invalid action or binding index.");
            return;
        }

        if (action.bindings[bindingIndex].isComposite)
        {
            Debug.LogError($"❌ Cannot rebind composite binding (like 'Move: 2D Vector') directly.\nPlease rebind specific parts (Up/Down/Left/Right) instead.");
            return;
        }

        rebindButton.interactable = false;
        bindingText.text = "...";

        action.Disable(); // Tạm thời disable để tránh lỗi

        action.PerformInteractiveRebinding(bindingIndex)
            .WithControlsExcluding("Mouse")
            .OnComplete(operation =>
            {
                operation.Dispose();
                UpdateBindingDisplay();
                SaveBindingOverride();
                action.Enable();
                rebindButton.interactable = true;
            })
            .Start();
    }


    public void SaveBindingOverride()
    {
        if (action == null) return;
        string key = action.actionMap.name + "/" + action.name;
        string json = action.actionMap.SaveBindingOverridesAsJson();
        PlayerPrefs.SetString(key, json);
        PlayerPrefs.Save();
    }

    public void LoadBindingOverride()
    {
        if (action == null) return;
        string key = action.actionMap.name + "/" + action.name;
        if (PlayerPrefs.HasKey(key))
        {
            string json = PlayerPrefs.GetString(key);
            action.actionMap.LoadBindingOverridesFromJson(json);
            UpdateBindingDisplay();
        }
    }

    public void ResetBindingOverride()
    {
        if (action == null) return;
        action.RemoveBindingOverride(bindingIndex);
        UpdateBindingDisplay();
        PlayerPrefs.DeleteKey(action.actionMap.name + "/" + action.name);
    }
}
