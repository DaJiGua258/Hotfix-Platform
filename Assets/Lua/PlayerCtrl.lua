-- 玩家控制器
require("FSM/StateMachine")
require("FSM/IdleState")
require("FSM/RunState")
require("FSM/JumpState")
require("FSM/FallState")

Object:subClass("PlayerCtrl")

function PlayerCtrl:new()
    local obj = Object.new(self)

    obj.motor = PlayerMotor  -- 玩家物理系统
    obj.input = InputProxy  -- 玩家输入

    obj.stateMachine = StateMachine:new()  -- 状态机

    -- 状态类
    obj.idleState = IdleState:new()
    obj.runState = RunState:new()
    obj.fallState = FallState:new()
    obj.jumpState = JumpState:new()

    obj.stateMachine:ChangeState(obj.idleState, obj)  -- 初始化状态机
    return obj
end

function PlayerCtrl:Update()
    self.stateMachine:Update(self)
end

return PlayerCtrl
