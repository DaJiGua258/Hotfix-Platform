-- 跑步状态
require("FSM/State")

State:subClass("RunState")

function RunState:OnEnter(ctrl)
    print("[State] Run")
end

function RunState:OnUpdate(ctrl)

    local move = ctrl.input.MoveVec

    -- 状态切换
    if move.x == 0 and move.y == 0 then
        ctrl.stateMachine:ChangeState(ctrl.idleState, ctrl)
        return
    end

    -- 移动
    ctrl.motor:Move(move)

    -- 跳跃
    if ctrl.input.IsJumpPressed then
        ctrl.stateMachine:ChangeState(ctrl.jumpState, ctrl)
        return
    end
end

function RunState:OnExit(ctrl)

end

return RunState
