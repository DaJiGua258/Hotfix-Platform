-- 待机状态
require("FSM/State")

State:subClass("IdleState")

function IdleState:OnEnter(ctrl)
    print("[State] Idle")
    ctrl.motor:ClearVelocity()
end

function IdleState:OnUpdate(ctrl)
    local move = ctrl.input.MoveVec

    -- 状态切换
    if move.x ~= 0 or move.y ~= 0 then
        ctrl.stateMachine:ChangeState(ctrl.runState, ctrl)
    end

    -- 跳跃
    if ctrl.input.IsJumpPressed then
        ctrl.stateMachine:ChangeState(ctrl.jumpState, ctrl)
        return
    end
end

function IdleState:OnExit(ctrl)
end

return IdleState
