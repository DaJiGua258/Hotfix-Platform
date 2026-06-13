-- 跳跃状态
require("FSM/State")

State:subClass("JumpState")

function JumpState:OnEnter(ctrl)
    print("[State] Jump")
    ctrl.motor:Jump()
end

function JumpState:OnUpdate(ctrl)
    -- 达到最高点开始，切换下落状态
    if ctrl.motor.VerticalVelocity <= 0 then
        ctrl.stateMachine:ChangeState(ctrl.fallState, ctrl)
        return
    end

    -- 空中可移动（水平控制）
    local move = ctrl.input.MoveVec

    if move.x ~= 0 or move.y ~= 0 then
        ctrl.motor:Move(move)
    end
end

function JumpState:OnExit(ctrl)
end

return JumpState
