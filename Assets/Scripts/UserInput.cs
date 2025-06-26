using UnityEngine;
using UnityEngine.InputSystem;

public class UserInput : MonoBehaviour, PlayerInputActions.IPlayerActions
{
    public static UserInput Instance { get; private set; }

    public PlayerInputActions playerInput { get; private set; }  // THÊM NÈ

    public Vector2 movementInput { get; private set; }
    public bool jumpInput { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        playerInput = new PlayerInputActions(); // Tạo và lưu dùng chung
        playerInput.Player.SetCallbacks(this);
        foreach (var map in playerInput.asset.actionMaps)
        {
            string key = map.name;
            if (PlayerPrefs.HasKey(key))
            {
                string json = PlayerPrefs.GetString(key);
                map.LoadBindingOverridesFromJson(json);
            }
        }

    }

    private void OnEnable() => playerInput.Player.Enable();
    private void OnDisable() => playerInput.Player.Disable();

    public void OnMove(InputAction.CallbackContext context) =>
        movementInput = context.ReadValue<Vector2>();

    public void OnJump(InputAction.CallbackContext context) =>
        jumpInput = context.performed;
}
