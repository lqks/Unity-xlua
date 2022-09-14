using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;
using System.IO;
using System;
using System.Runtime.InteropServices;

public class XLuaManager : MonoBehaviour
{
    public LuaEnv XLuaEnv;
    private Action luaUpdate = null;
    private Action luaStart = null;
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("初始化游戏 Start");
        StartCoroutine(InitGame());
    }

    // Update is called once per frame
    void Update()
    {
        if (luaUpdate != null)
        {
            luaUpdate();
        }
    }

    //初始化游戏
    IEnumerator InitGame()
    {
        //TODO:这里加载资源

        yield return false;
        InitLuaEnv();
    }

    void InitLuaEnv()
    {
        Debug.Log("------------------InitLuaEnv---------------------");
        XLuaEnv = new LuaEnv();
        if (XLuaEnv != null)
        {
            XLuaEnv.AddLoader(CustomLoader);
        }

        XLuaEnv.DoString("require('main')");

        //第一种写法
        XLuaEnv.Global.Get("Update", out luaUpdate);
        //第二种写法
        luaStart = XLuaEnv.Global.Get<Action>("Start");

        if(luaStart != null )
        {
            luaStart();
        }
    }

    //自定义加载Lua脚本
    public static byte[] CustomLoader(ref string filepath)
    {
        filepath = "Assets/Lua/" + filepath.Replace(".", "/") + ".lua";
        #if UNITY_EDITOR
            return SafeReadAllBytes(filepath);
        #else
            //TODO真机环境用另外的加载方式。一般情况下是读取AB的
        #endif
    }

    static byte[] SafeReadAllBytes( string infile)
    {
        try{
            if(string.IsNullOrEmpty(infile))
            {
                return null;
            }
            if(!File.Exists(infile))
            {
                return null;
            }

            File.SetAttributes(infile, FileAttributes.Normal);
            return File.ReadAllBytes(infile);
        }
        catch(System.Exception ex)
        {
            Debug.Log("读取Lua脚本失败，脚本 ： "+ infile);
            return null;
        }
    }

}
