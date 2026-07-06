-- 突进状态
require("FSM/State")

State:subClass("DashState")

local DASH_SPEED = 15       -- 突进初速度
local DASH_DURATION = 0.25  -- 突进持续时间（秒）

function DashState:OnEnter(ctrl)
    -- 冲刺加速
    ctrl.motor:SetSprint(true)

    -- 朝角色面朝方向突进（transform.forward 已由 Motor 保持为相机相对方向）
    local forward = ctrl.motor.transform.forward
    ctrl.dashDirection = Vector2(forward.x, forward.z)
    ctrl.dashStartTime = CS.UnityEngine.Time.time

    -- 初始速度爆发
    ctrl.motor:SetVelocity(Vector3(forward.x, 0, forward.z) * DASH_SPEED)

    ctrl.motor:PlayDashVFX()
    _G.AudioMgr:PlayJump()
end

function DashState:OnUpdate(ctrl)
    -- 保持移动方向
    ctrl.motor:Move(ctrl.dashDirection)

    -- 突进结束，根据状态切换
    if CS.UnityEngine.Time.time - ctrl.dashStartTime >= DASH_DURATION then
        if ctrl.motor.IsGrounded then
            if ctrl.input.MoveVec.magnitude > 0 then
                ctrl.stateMachine:ChangeState(ctrl.runState, ctrl)
            else
                ctrl.stateMachine:ChangeState(ctrl.idleState, ctrl)
            end
        else
            ctrl.stateMachine:ChangeState(ctrl.fallState, ctrl)
        end
    end
end

function DashState:OnExit(ctrl)
    ctrl.motor:SetSprint(false)
end

return DashState
