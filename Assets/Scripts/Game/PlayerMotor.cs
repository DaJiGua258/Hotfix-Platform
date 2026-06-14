using UnityEngine;
using DG.Tweening;
using XLua;

/// <summary>
/// 角色物理运动，纯手动计算不依赖 Rigidbody
/// Lua 通过公开方法控制移动和跳跃，不做逻辑决策
/// </summary>
[Hotfix]
public class PlayerMotor : MonoBehaviour
{
    #region ----- 序列化参数 -------------------------

    [Header("移动参数")]
    [SerializeField] private float _moveSpeed = 5f;  // 移动速度（米/秒）
    [SerializeField] private float _sprintMultiplier = 1.5f;  // 冲刺倍率
    [SerializeField] private float _acceleration = 10f;  // 地面加速平滑度
    [SerializeField] private float _airControl = 5f;  // 空中控制平滑度

    [Header("跳跃参数")]
    [SerializeField] private float _jumpForce = 8f;  // 跳跃初速度
    [SerializeField] private float _gravity = -20f;  // 重力加速度（负值）

    [Header("地面检测")]
    [SerializeField] private float _groundCheckDistance = 0.12f;  // 检测射线长度（仅 Gizmo 显示用）

    [Header("旋转参数")]
    [SerializeField] private float _rotationSpeed = 15f;  // 转向平滑速度

    [Header("动画参数")]
    [SerializeField] private Animator _animator;  // Animator 引用
    [Header("脚印粒子")]
    [SerializeField] private ParticleSystem _footstepVFX;  // 地面时自动开启 Emission
    [Header("落地粒子")]
    [SerializeField] private ParticleSystem _landVFX;  // 落地瞬间播放

    [Header("跳跃与落地拉伸/挤压")]
    [SerializeField] private Transform _squashTarget;  // 弹跳形变目标（为空则用自身）
    [SerializeField] private float _squashEaseIn = 0.05f;  // 形变渐入时间（跳跃/落地）
    [SerializeField] private float _squashDuration = 0.2f;  // 弹性恢复时间
    [SerializeField] private Vector3 _stretchScale = new Vector3(0.75f, 1.25f, 0.75f);  // 起跳拉伸倍数
    [SerializeField] private Vector3 _squashScale = new Vector3(1.25f, 0.75f, 1.25f);  // 落地挤压倍数

    [Header("移动拉伸/挤压")]
    [SerializeField] private float _bobEaseIn = 0.03f;  // 形变渐入时间（脚步）
    [SerializeField] private float _bobDuration = 0.15f;  // bob 恢复时间
    [SerializeField] private Vector3 _bobScale = new Vector3(1.08f, 0.92f, 1.08f);  // 每步按压倍数

    #endregion

    #region ----- 私有字段 -------------------------

    private CharacterController _charController;
    private Transform _mainCamera;
    private Transform _squashTransform;  // 实际执行形变的 Transform
    private Vector3 _velocity;  // 当前速度向量
    private Vector3 _moveInput;  // 当前帧移动输入方向
    private bool _isSprinting;
    private bool _isGrounded;
    private bool _wasGrounded;  // 上一帧地面状态，用于检测落地瞬间
    private Vector3 _originalScale;  // 初始缩放，squash/stretch 恢复目标
    private Tween _scaleTween;  // DoTween 缩放动画引用

    #endregion

    #region ----- Lua 只读属性 -------------------------

    public bool IsGrounded => _isGrounded;
    public float VerticalVelocity => _velocity.y;  // Lua 用于判断 Jumping/Falling 转换
    public Vector3 CurrentVelocity => _velocity;

    #endregion

    #region ----- Unity 生命周期 -------------------------

    void Awake()
    {
        _charController = GetComponent<CharacterController>();
        if (_charController == null)
            _charController = gameObject.AddComponent<CharacterController>();

        Camera cam = Camera.main;
        if (cam != null)
            _mainCamera = cam.transform;

        if (_animator == null)
            _animator = GetComponent<Animator>();

        _squashTransform = _squashTarget != null ? _squashTarget : transform;
        _originalScale = _squashTransform.localScale;
    }

    void Update()
    {
        // 每帧按顺序执行物理运算
        CheckGround();
        UpdateFootstepVFX();
        ApplyGravity();
        ApplyHorizontalMovement();
        ApplyMovement();
        ApplyRotation();
        ApplyAnimator();
    }

    void OnTriggerEnter(Collider other)
    {
        // 将碰撞事件转发给 Lua 处理（由 Lua 判断具体逻辑）
        LuaTable global = LuaMgr.GetInstance().Global;
        LuaFunction fn = global.Get<LuaFunction>("OnPlayerTrigger");
        if (fn != null)
            fn.Call(other.gameObject);
    }

    #endregion

    #region ----- 物理运算 -------------------------

    /// <summary>
    /// CharacterController.isGrounded 检测地面
    /// 落地时归零垂直速度并贴地
    /// </summary>
    private void CheckGround()
    {
        _isGrounded = _charController.isGrounded;

        // 落地处理：归零垂直速度
        if (_isGrounded && _velocity.y < 0)
        {
            float fallSpeed = _velocity.y;  // 记录下落速度，用于判断是否为真正落地
            _velocity.y = -2;  // 贴地力

            // 落地 squash → 渐入形变 → 弹性回正
            // fallSpeed < -3 过滤掉地面抖动导致的误触发
            if (!_wasGrounded && fallSpeed < -3f)
            {
                _scaleTween.Kill();
                var seq = DOTween.Sequence();
                seq.Append(_squashTransform.DOScale(Vector3.Scale(_originalScale, _squashScale), _squashEaseIn));
                seq.Append(_squashTransform.DOScale(_originalScale, _squashDuration).SetEase(Ease.OutBack));
                _scaleTween = seq;

                if (_landVFX != null)
                    _landVFX.Play();
            }
        }

        _wasGrounded = _isGrounded;
    }

    /// <summary>
    /// 非地面时叠加自定义重力
    /// </summary>
    private void ApplyGravity()
    {
        if (!_isGrounded)
            _velocity.y += _gravity * Time.deltaTime;
    }

    /// <summary>
    /// 根据输入和目标速度做平滑加速/减速
    /// 地面使用 _acceleration，空中使用 _airControl
    /// </summary>
    private void ApplyHorizontalMovement()
    {
        float speed = _isSprinting ? _moveSpeed * _sprintMultiplier : _moveSpeed;
        Vector3 target = _moveInput * speed;
        float smooth = (_isGrounded ? _acceleration : _airControl) * Time.deltaTime;

        // 用 Lerp 实现平滑过渡，避免速度突变
        _velocity.x = Mathf.Lerp(_velocity.x, target.x, smooth);
        _velocity.z = Mathf.Lerp(_velocity.z, target.z, smooth);
    }

    /// <summary>
    /// 将最终速度通过 CharacterController 应用（自动处理碰撞）
    /// </summary>
    private void ApplyMovement()
    {
        _charController.Move(_velocity * Time.deltaTime);
    }

    /// <summary>
    /// 朝水平速度方向平滑旋转
    /// </summary>
    private void ApplyRotation()
    {
        Vector3 horizontalVelocity = new Vector3(_velocity.x, 0, _velocity.z);
        if (horizontalVelocity.magnitude > 0.01f)
        {
            Quaternion target = Quaternion.LookRotation(horizontalVelocity);
            transform.rotation = Quaternion.Slerp(transform.rotation, target, _rotationSpeed * Time.deltaTime);
        }
    }

    /// <summary>
    /// 每帧同步物理参数到 Animator
    /// </summary>
    private void ApplyAnimator()
    {
        if (_animator == null) return;

        Vector3 horizontalVelocity = new Vector3(_velocity.x, 0, _velocity.z);
        _animator.SetFloat("Speed", horizontalVelocity.magnitude);
        _animator.SetFloat("SpeedY", _velocity.y);
        _animator.SetBool("IsGrounded", _isGrounded);
    }

    /// <summary>
    /// 地面时开启脚印粒子，空中时关闭
    /// </summary>
    private void UpdateFootstepVFX()
    {
        if (_footstepVFX == null) return;

        var emission = _footstepVFX.emission;
        emission.enabled = _isGrounded;
    }

    #endregion

    #region ----- Lua 调用接口 -------------------------

    /// <summary>
    /// 设置水平移动方向
    /// </summary>
    /// <param name="x">左右方向 -1~1</param>
    /// <param name="z">前后方向 -1~1</param>
    public void Move(Vector2 input)
    {
        if (_mainCamera != null)
        {
            // 以相机朝向为参考系转换输入方向
            Vector3 forward = _mainCamera.forward;
            forward.y = 0;
            forward.Normalize();

            Vector3 right = _mainCamera.right;
            right.y = 0;
            right.Normalize();

            _moveInput = forward * input.y + right * input.x;
        }
        else
        {
            // 没有主相机则回退到世界坐标
            _moveInput = new Vector3(input.x, 0, input.y);
        }

        // 防止斜向移动速度超标
        if (_moveInput.magnitude > 1f)
            _moveInput.Normalize();
    }

    /// <summary>
    /// 跳跃，仅在地面时生效
    /// </summary>
    public void Jump()
    {
        if (_isGrounded)
        {
            _velocity.y = _jumpForce;
            _isGrounded = false;

            // 起跳 stretch → 渐入形变 → 弹性回正
            _scaleTween.Kill();
            var seq = DOTween.Sequence();
            seq.Append(_squashTransform.DOScale(Vector3.Scale(_originalScale, _stretchScale), _squashEaseIn));
            seq.Append(_squashTransform.DOScale(_originalScale, _squashDuration).SetEase(Ease.OutBack));
            _scaleTween = seq;
        }
    }

    /// <summary>
    /// 脚步触发的 bob，给 Animation Event 调用
    /// </summary>
    public void OnStep()
    {
        _scaleTween.Kill();
        var seq = DOTween.Sequence();
        seq.Append(_squashTransform.DOScale(Vector3.Scale(_originalScale, _bobScale), _bobEaseIn));
        seq.Append(_squashTransform.DOScale(_originalScale, _bobDuration).SetEase(Ease.OutBack));
        _scaleTween = seq;
    }

    /// <summary>
    /// 设置冲刺状态
    /// </summary>
    /// <param name="sprint">是否冲刺</param>
    public void SetSprint(bool sprint)
    {
        _isSprinting = sprint;
    }

    /// <summary>
    /// 传送角色到指定位置（重生用）
    /// </summary>
    /// <param name="pos">目标位置</param>
    public void Teleport(Vector3 pos)
    {
        transform.position = pos;
        _velocity = Vector3.zero;
        Move(Vector2.zero);
    }

    /// <summary>
    /// 清空当前速度
    /// </summary>
    public void ClearVelocity()
    {
        _velocity = Vector3.zero;
        Move(Vector2.zero);
    }

    #endregion

    #region ----- 编辑器辅助 -------------------------

    void OnDrawGizmosSelected()
    {
        if (_charController == null) return;

        Vector3 origin = transform.position + Vector3.down * (_charController.height * 0.5f);

        // 绿色=在地面，红色=在空中
        Gizmos.color = _isGrounded ? Color.green : Color.red;
        Gizmos.DrawLine(origin, origin + Vector3.down * _groundCheckDistance);
    }

    #endregion
}
