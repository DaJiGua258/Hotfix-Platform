-- 统一帧更新调度器
-- 所有需要每帧更新的模块都通过 RegisterUpdate 注册

local GameLoop = {}

local startList = {}
local updateList = {}
local destroyList = {}

-- 周期函数注册
function GameLoop:RegisterLoop(start, update, destroy)
    table.insert(startList, start)
    table.insert(updateList, update)
    table.insert(destroyList, destroy)
end


-- 遍历注册的周期函数
function GameLoop:LoopList(list)
    for _, fn in ipairs(list) do
        fn()
    end
end

function Start()
    GameLoop:LoopList(startList)
end

function Update()
    GameLoop:LoopList(updateList)
end

function Destory()
    GameLoop:LoopList(destroyList)
end


return GameLoop

-- === 各子模块在此加载并注册 ===
-- 示例：
-- local Player = require("Test")
-- RegisterUpdate(function() Player:Update() end)
--
-- local Enemy = require("Enemy")
-- RegisterUpdate(function() Enemy:Update() end)
