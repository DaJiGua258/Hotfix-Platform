-- 初始化各种需要全局使用的类
print("----- 执行 Main -----")

require("Object")

-- Unity相关
GameObject = CS.UnityEngine.GameObject
Transform = CS.UnityEngine.Transform
RectTransform = CS.UnityEngine.RectTransform
Vector3 = CS.UnityEngine.Vector3
Vector2 = CS.UnityEngine.Vector2
Input = CS.UnityEngine.Input

-- UI
UI = CS.UnityEngine.UI
Image = UI.Image
Sprite = CS.UnityEngine.Sprite
Text = UI.Text
Button = UI.Button
Toggle = UI.Toggle
ScrollRect = UI.ScrollRect
UIBehaviour = CS.UnityEngine.EventSystems.UIBehaviour

-- 资源相关
TextAsset = CS.UnityEngine.TextAsset
Resource = CS.UnityEngine.Resource

-- 场景相关
SceneManager = CS.UnityEngine.SceneManagement.SceneManager

-- 存储
PlayerPrefs =  CS.UnityEngine.PlayerPrefs

--自己写的C#脚本相关
--直接得到AB包资源管理器的 单例对象
ABMgr = CS.ABMgr.GetInstance()

-- 加载Lua脚本
-- 周期函数循环
local loop = require("GameLoop")

-- local test = require("Test")
-- loop:RegisterLoop(
--     function() test:Start() end,
--     function() test:Update() end,
--     function() test:OnDestory() end
-- )

Canvas = GameObject.Find("Canvas").transform
require("UI/BasePanel")

-- 当前场景是main场景时才加载main相关UI
if SceneManager.GetActiveScene().name == "Main" then

    -- 初始化Lua脚本
	require("UI/MainPanel")
    loop:RegisterLoop(nil, function() MainPanel:Update() end, nil)
    require("UI/LevelSelect")
    require("UI/SceneData")

    -- 初始化数据
    -- SceneData:ReSetData()

    -- 初始化资源
	MainPanel:Init("panel_Main")

-- 关卡加载
else
    -- 关卡数据加载
    require("UI/SceneData")
    require("UI/GameOver")
    require("UI/GamePause")
    GamePause:Init("panel_Pause")
    GameOver:Init("panel_GameOver")

    GamePause:Hide()
    GameOver:Hide()

    -- 游戏内UI
    require("UI/GamePanel")
    GamePanel:Init("panel_Game")
    loop:RegisterLoop(nil, function() GamePanel:Update() end, nil)

    -- 初始化关卡数据（金币等）
    SceneData:InitData()

    -- 计时更新
    loop:RegisterLoop(nil, function() SceneData:Update() end, nil)

    -- 玩家控制
    local playerCtrl = require("PlayerCtrl"):new()
    loop:RegisterLoop(nil, function() playerCtrl:Update() end, nil)

end





-- 玩家控制
-- local playerCtrl = require("PlayerCtrl"):new()
-- loop:RegisterLoop(nil, function() playerCtrl:Update() end, nil)


