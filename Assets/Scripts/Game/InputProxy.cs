using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 每帧收集玩家输入缓存到属性，供 Lua 读取
/// </summary>
public class InputProxy : MonoBehaviour
{
    private PlayerInput _playerInput;
    private InputActionAsset _asset;

    public Vector2 MoveVec => _asset["Move"].ReadValue<Vector2>();
    public bool IsJumpPressed => _asset["Jump"].WasPressedThisFrame();
    public bool IsSprintHeld => _asset["Sprint"].ReadValue<float>() > 0.5f;
    public bool IsPausePressed => _asset["Pause"].WasPressedThisFrame();
    public bool IsDashPressed => _asset["Sprint"].WasPressedThisFrame();

    void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
        _asset = _playerInput.actions;
    }

    void Update()
    {
        // if(MoveVec != Vector2.zero)
        // {
        //     Debug.Log(MoveVec);
        // }
    
        // Input.GetKeyDown(KeyCode.Escape);
        // Instantiate
        // Text text = GameObject.Find("Text").GetComponent<Text>();
        // text.text = MoveVec.ToString();
        // Sprite sprite = GameObject.Find("Sprite").GetComponent<Image>().sprite;
        // SceneManager.LoadScene
    }
}
