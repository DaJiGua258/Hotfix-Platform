-- 渐入渐出控制
local Fade = {}
local fadeImage = nil

function Fade:Init()
    -- 从 ui 包加载遮罩图片，挂到 Canvas 下
    local fadeObj = ABMgr:LoadRes("ui", "img_Fade", typeof(GameObject))
    fadeObj.transform:SetParent(Canvas, false)
    fadeImage = fadeObj:GetComponent(typeof(Image))
    fadeImage.gameObject:SetActive(false)
end

-- 渐入：透明度 0 → 1，完成后触发回调
function Fade:FadeIn(duration, callback)
    if fadeImage then
        CS.FadeHelper.FadeIn(fadeImage, duration or 0.5, callback)
    elseif callback then
        callback()
    end
end

-- 渐出：透明度 1 → 0，完成后隐藏并触发回调
function Fade:FadeOut(duration, callback)
    if fadeImage then
        CS.FadeHelper.FadeOut(fadeImage, duration or 0.5, callback)
    elseif callback then
        callback()
    end
end

return Fade
