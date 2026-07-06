-- 金币旋转控制

function InitCoins(GameLoop)
    local coins = CS.UnityEngine.GameObject.FindGameObjectsWithTag("Coin")

    GameLoop:RegisterLoop(
        nil,
        function()
            local dt = CS.UnityEngine.Time.deltaTime
            for i = 0, coins.Length - 1 do
                local coin = coins[i]
                if coin and coin.activeSelf then
                    coin.transform:Rotate(0, 180 * dt, 0)
                end
            end
        end,
        nil
    )
end

return InitCoins