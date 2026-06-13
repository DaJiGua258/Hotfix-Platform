require("FSM/State")

State:subClass("FallState")

function FallState:OnEnter(ctrl)
    print("[State] Fall")
end

function FallState:OnUpdate(ctrl)
    if ctrl.motor.IsGrounded then
        ctrl.stateMachine:ChangeState(ctrl.idleState, ctrl)
    end
end

function FallState:OnExit(ctrl)

end