using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 游戏启动入口，负责初始化 Lua、注册 C# 对象、驱动每帧更新
/// </summary>
public class GameManager : SingletonAutoMono<GameManager>
{
    private void Start()
    {
        // 初始化 Lua 环境
        LuaMgr.GetInstance().Init();

        SetToLua("PlayerMotor", typeof(PlayerMotor));
        SetToLua("InputProxy", typeof(InputProxy));
        LuaMgr.GetInstance().Global.Set("AudioMgr", AudioMgr.GetInstance());

        // 加载 Lua 入口
        LuaMgr.GetInstance().DoLuaFile("Main");

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 跨场景后刷新 C# 对象引用，Lua 状态保持不变
        SetToLua("PlayerMotor", typeof(PlayerMotor));
        SetToLua("InputProxy", typeof(InputProxy));
    }

    private void SetToLua(string name, System.Type type)
    {
        var o = FindObjectOfType(type);
        LuaMgr.GetInstance().Global.Set(name, o);
    }

}
