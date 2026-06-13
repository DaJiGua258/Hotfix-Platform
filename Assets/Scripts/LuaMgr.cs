using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using XLua;

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
        
        //添加重定向委托函数
        luaEnv.AddLoader(MyCustomLoader);
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
        //改为我们的AB包管理器加载
        TextAsset file2 = ABMgr.GetInstance().LoadRes<TextAsset>("lua", filepath + ".lua");
        if (file2 != null)
            return file2.bytes;
        else
            Debug.Log("MyCustomLoaderFormAB重定向失败");
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
