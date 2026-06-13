-- 初始化各种需要全局使用的类
print("----- 执行 Main -----")

require("Object")

-- Unity相关
GameObject = CS.UnityEngine.GameObject
Transform = CS.UnityEngine.Transform
RectTransform = CS.UnityEngine.RectTransform
Vector3 = CS.UnityEngine.Vector3
Vector2 = CS.UnityEngine.Vector2

-- UI
UI = CS.UnityEngine.UI
Image = UI.Image
Text = UI.Text
Button = UI.Button
Toggle = UI.Toggle
ScrollRect = UI.ScrollRect
UIBehaviour = CS.UnityEngine.EventSystems.UIBehaviour

-- 资源相关
TextAsset = CS.UnityEngine.TextAsset
Resource = CS.UnityEngine.Resource


--自己写的C#脚本相关
--直接得到AB包资源管理器的 单例对象
ABMgr = CS.ABMgr.GetInstance()

-- 加载Lua脚本
-- 周期函数循环
local loop = require("GameLoop")

local test = require("Test")
-- loop:RegisterLoop(
--     function() test:Start() end,
--     function() test:Update() end,
--     function() test:OnDestory() end
-- )

-- 玩家控制
local playerCtrl = require("PlayerCtrl"):new()
loop:RegisterLoop(nil, function() playerCtrl:Update() end, nil)


