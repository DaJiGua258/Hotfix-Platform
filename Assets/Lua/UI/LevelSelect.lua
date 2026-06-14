BasePanel:subClass("LevelSelect")

function LevelSelect:Init()
    
    if self.base.isInitEvent then return end 

    -- self.base.Init(self, name)
    self.content = self.panelObj.transform:Find("Content")

    -- 加载资源
    local item = ABMgr:LoadRes("ui", "btn_Level")

    -- 加载两个image
    local star = ABMgr:LoadRes("ui", "star", typeof(Sprite))
    local star_outline = ABMgr:LoadRes("ui", "star_outline", typeof(Sprite))

    -- 遍历关卡信息
    for _, v in ipairs(SceneData) do
        
        local score = PlayerPrefs.GetInt(v)  -- 获取分数

        -- 如果未解锁，直接跳过
        if score >= 0 then 
            -- 实例化资源
            local obj = CS.UnityEngine.Object.Instantiate(item)
            
            obj.transform:SetParent(self.content)

            -- 添加监听
            obj.gameObject:GetComponent(typeof(Button)).onClick:AddListener(
                function() 
                    print("加载场景: " .. v)
                    SceneManager.LoadScene(v)
                    CurrentLevel = v
                end
            )
            -- 设置组件
            obj.transform:Find("Text").gameObject:GetComponent(typeof(Text)).text = v

            local scoreRoot = obj.transform:Find("Score")
            -- 遍历所有获取到的 star 组件，
            local scoreImgs = scoreRoot.transform:GetComponentsInChildren(typeof(Image))
            print(v .. "  " .. score)
            
            -- 遍历设置组件
            for i = 0, scoreImgs.Length - 1 do
                if score > 0 then
                    scoreImgs[i].sprite = star
                    score = score - 1
                else
                    scoreImgs[i].sprite = star_outline
                end
            end

        end 
    end

    self.base.isInitEvent = true
end

function LevelSelect:Show()
    self.base:Show("panel_LevelSelect")
    self:Init()
end

function LevelSelect:Hide()
    self.base:Hide()
end
