using UnityEngine;
using UnityEngine.SceneManagement;
using ABToolPackage;

/// <summary>
/// 游戏启动入口，负责初始化 Lua、注册 C# 对象、驱动每帧更新
/// </summary>
public class GameManager : SingletonAutoMono<GameManager>
{
    private void Awake()
    {
        // 防止场景中放置的 GameManager 重复创建
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // 先检查并下载远端 AB 包更新，完成后才初始化 Lua
        ABDownload.Instance.CheckUpdate(() =>
        {
            // 初始化 Lua 环境
            LuaMgr.GetInstance().Init();

            SetToLua("PlayerMotor", typeof(PlayerMotor));
            SetToLua("InputProxy", typeof(InputProxy));
            LuaMgr.GetInstance().Global.Set("AudioMgr", AudioMgr.GetInstance());

            // 加载 Lua 入口
            LuaMgr.GetInstance().DoLuaFile("Main");
        });

        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private void OnSceneUnloaded(Scene scene)
    {
        // 旧场景已卸载，启动协程重建 Lua 环境
        LuaMgr.GetInstance().DoRestart(() =>
        {
            // 回调中重新注册 C# 对象并加载 Lua 入口
            SetToLua("PlayerMotor", typeof(PlayerMotor));
            SetToLua("InputProxy", typeof(InputProxy));
            LuaMgr.GetInstance().Global.Set("AudioMgr", AudioMgr.GetInstance());
            LuaMgr.GetInstance().DoLuaFile("Main");
        });
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 新场景加载完毕，仅刷新 C# 对象引用（Lua 由 OnSceneUnloaded 中的协程重建）
        SetToLua("PlayerMotor", typeof(PlayerMotor));
        SetToLua("InputProxy", typeof(InputProxy));
    }

    private void SetToLua(string name, System.Type type)
    {
        var o = FindObjectOfType(type);
        LuaMgr.GetInstance().Global.Set(name, o);
    }

}
