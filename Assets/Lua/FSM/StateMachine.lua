-- 状态机，管理状态切换和委托更新
require("FSM/State")

Object:subClass("StateMachine")

function StateMachine:new()
    local obj = Object.new(self)
    obj.currentState = nil
    return obj
end

function StateMachine:ChangeState(newState, ctrl)

    if self.currentState == newState then 
        return
    end

    if self.currentState then
        self.currentState:OnExit(ctrl)
    end

    self.currentState = newState

    if newState then
        newState:OnEnter(ctrl)
    end
end

function StateMachine:Update(ctrl)
    if self.currentState then
        self.currentState:OnUpdate(ctrl)
    end
end
