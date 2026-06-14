BasePanel:subClass("GameOver")

function GameOver:Init(name)
    self.base.Init(self, name)

    self:GetComp("btn_ReStart", "Button").onClick:AddListener(
        function()
            CS.UnityEngine.Time.timeScale = 1
            isGameOver = false
            CS.UnityEngine.SceneManagement.SceneManager.LoadScene(CS.UnityEngine.SceneManagement.SceneManager.GetActiveScene().name)
        end
    )

    self:GetComp("btn_ReturnMain", "Button").onClick:AddListener(
        function()
            CS.UnityEngine.Time.timeScale = 1
            isGameOver = false
            CS.UnityEngine.SceneManagement.SceneManager.LoadScene("Main")
        end
    )

    self:GetComp("btn_NextLevel", "Button").onClick:AddListener(
        function()
            print("下一关")
        end
    )
end

function GameOver:Show()
    self.base.Show(self, "panel_GameOver")

    -- 显示通关时间和金币
    local minutes = math.floor(CurLevelData.time / 60)
    local seconds = math.floor(CurLevelData.time % 60)
    local timeStr = string.format("完成时间：%02d分%02d秒", minutes, seconds)
    local coinStr = "金币收集：" .. CurLevelData.coin
    self.panelObj.transform:Find("Content/LevelTitle/text_Score"):GetComponent(typeof(Text)).text = timeStr .. "\n" .. coinStr

    -- 根据分数显示星星
    local scoreRoot = self.panelObj.transform:Find("Content/Score")
    for i = 0, scoreRoot.childCount - 1 do
        scoreRoot:GetChild(i).gameObject:SetActive(i < CurLevelData.score)
    end
end

function GameOver:Hide()
    self.base.Hide(self)
end
