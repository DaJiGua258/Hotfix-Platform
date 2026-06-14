BasePanel:subClass("GamePanel")

-- 虚拟相机名（场景中 CM vcam1）
local vcamName = "CM vcam1"

function GamePanel:Init(name)
    self.base.Init(self, name)
    self.coinText = self.panelObj.transform:Find("Image/text_Coin"):GetComponent(typeof(Text))
    self.coinText.text = "× " .. CurLevelData.coin
end

function GamePanel:AddCoin()
    self.coinText.text = "× " .. CurLevelData.coin
end

function GamePanel:Update()
    if Input.GetKeyDown(CS.UnityEngine.KeyCode.Escape) then
        if self.isPause then
            GamePause:Hide()
            CS.UnityEngine.Time.timeScale = 1
            self:SetVCamActive(true)
            self.isPause = false
        else
            GamePause:Show()
            CS.UnityEngine.Time.timeScale = 0
            self:SetVCamActive(false)
            self.isPause = true
        end
    end
end

function GamePanel:SetVCamActive(active)
    local go = CS.UnityEngine.GameObject.Find(vcamName)
    if go then
        go:GetComponent("CinemachineVirtualCamera").enabled = active
    end
end
