using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using XLua;
using DG.Tweening;

/// <summary>
/// Lua管理器
/// </summary>
public class LuaMgr : BaseManager<LuaMgr>
{
    private LuaEnv luaEnv;
    private Action luaStart;
    private Action luaUpdate;
    private Action luaDestroy;

    private LuaLoopDriver driver;

    public void Init()
    {
        //唯一的解析器
        luaEnv = new LuaEnv();

        // 注册 AB 场景加载函数到 Lua 全局，支持热更场景切换
        luaEnv.Global.Set("LoadSceneFromAB",
            (Action<string, string>)((abName, sceneName) =>
                ABMgr.GetInstance().LoadSceneFromAB(abName, sceneName)));

        //添加重定向委托函数
        // luaEnv.AddLoader(MyCustomLoader);
        luaEnv.AddLoader(MyCustomLoaderFormAB);

        //加载统一帧更新调度器，缓存 Tick 函数
        luaEnv.DoString("require('GameLoop')");
        luaStart = luaEnv.Global.Get<Action>("Start");
        luaUpdate = luaEnv.Global.Get<Action>("Update");
        luaDestroy = luaEnv.Global.Get<Action>("Destory");

        //创建驱动对象，接入 Unity 生命周期
        GameObject go = new GameObject("LuaMgrDriver");
        GameObject.DontDestroyOnLoad(go);
        driver = go.AddComponent<LuaLoopDriver>();
    }

    public void Start()
    {
        luaStart?.Invoke();
    }

    /// <summary>
    /// 由驱动在每一帧调用
    /// </summary>
    public void Update()
    {
        luaUpdate?.Invoke();
        luaEnv?.Tick();
    }

    /// <summary>
    /// 触发 Lua 端 Destory 回调
    /// </summary>
    public void Destroy()
    {
        luaDestroy?.Invoke();
    }

    //Lua总表
    //用于之后 lua访问C#时 使用 通过总表获取lua中各种内容
    public LuaTable Global
    {
        get
        {
            return luaEnv.Global;
        }
    }

    private byte[] MyCustomLoader(ref string filepath)
    {
        //测试传入的参数是什么
        Debug.Log(filepath);
        //决定Lua文件所在路径
        string path = Application.dataPath + "/Lua/" + filepath + ".lua";
        //C#自带的文件读取类
        if (File.Exists(path))
        {
            return File.ReadAllBytes(path);
        }
        else
            Debug.Log("MyCustomLoader重定向失败");

        return null;
    }

    //再写一个Load 用于从AB包加载Lua文件
    private byte[] MyCustomLoaderFormAB(ref string filepath)
    {
        // 改为我们的AB包管理器加载
        // AB 包中 asset 为 flat 结构（无子目录），取文件名即可
        string fileName = Path.GetFileName(filepath);
        TextAsset file2 = ABMgr.GetInstance().LoadRes<TextAsset>("lua", fileName + ".lua.txt");
        if (file2 != null)
            return file2.bytes;
        else
        {
            Debug.Log("MyCustomLoaderFormAB重定向失败：" + filepath + " → " + fileName + ".lua.txt");
            ABMgr.GetInstance().PrintABAssets("lua");
        }
        return null;
    }

    /// <summary>
    /// 执行lua文件
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="formWhere"></param>
    public void DoLuaFile(string fileName, string formWhere = null)
    {
        string str = string.Format("require('{0}')", fileName);
        luaEnv.DoString(str);
    }

    //执行Lua脚本
    public void DoString(string luaScript, string fromWhere = null)
    {
        luaEnv.DoString(luaScript, fromWhere);
    }

    //释放垃圾
    public void Tick()
    {
        luaEnv.Tick();
    }

    //销毁
    public void Dispose()
    {
        Destroy();
        if (driver != null)
            GameObject.Destroy(driver.gameObject);
        luaUpdate = null;
        luaStart = null;
        luaDestroy = null;
        luaEnv.Tick();
        luaEnv.Dispose();
    }

    // 启动协程重建 Lua 环境（由 GameManager 在 OnSceneUnloaded 中调用）
    public void DoRestart(Action onCompleted)
    {
        if (driver != null)
            driver.StartCoroutine(RestartCoroutine(onCompleted));
    }

    private IEnumerator RestartCoroutine(Action onCompleted)
    {
        // 断开 Lua 函数引用
        luaStart = null;
        luaUpdate = null;
        luaDestroy = null;

        // 清理按钮监听（Button.onClick 可能持有 Lua 委托）
        foreach (var btn in GameObject.FindObjectsOfType<UnityEngine.UI.Button>(true))
            btn.onClick.RemoveAllListeners();

        // 释放所有 DOTween 回调
        DOTween.KillAll(false, false);

        // 销毁驱动（Destroy 在本帧末尾才真正执行）
        if (driver != null)
            GameObject.Destroy(driver.gameObject);

        // 等一帧，让 Destroy 队列处理完
        yield return null;

        // 双轮 GC：一轮回收 + 一轮回收 Finalizer 产生的新垃圾
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        GC.WaitForPendingFinalizers();

        luaEnv.Tick();
        luaEnv.Dispose();
        luaEnv = null;

        Init();

        // 通知调用方完成（GameManager 在此回调中 SetToLua + DoLuaFile）
        onCompleted?.Invoke();
    }
}

/// <summary>
/// LuaMgr 的 Unity 生命周期驱动
/// </summary>
public class LuaLoopDriver : MonoBehaviour
{
    void Start()
    {
        LuaMgr.GetInstance().Start();
    }

    void Update()
    {
        LuaMgr.GetInstance().Update();
    }

    void OnDestroy()
    {
        LuaMgr.GetInstance().Destroy();
    }
}
