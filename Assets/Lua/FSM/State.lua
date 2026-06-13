-- 状态基类，所有具体状态的父类
require("Object")

Object:subClass("State")

function State:OnEnter(ctrl)
end

function State:OnUpdate(ctrl)
end

function State:OnExit(ctrl)
end
