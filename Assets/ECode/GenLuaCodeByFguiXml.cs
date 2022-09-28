using System;
using System.Collections.Generic;
using System.Text;
using FairyGUI;
using UnityEngine;

#if UNITY_EDITOR

namespace XiahExcel
{
    public class GenLuaCodeByFguiXml
    {
        public static string CustomLogicCode = "";

        public static List<ChildCom> customComs = new List<ChildCom>();
        public static List<string> listItem = new List<string>();


        public static string writeFguiTpl(string comName, string type, UIPackage pkg, PackageItem item, List<ChildCom> childList)
        {
            string CodeAsComponent = "";
            customComs.Clear();

            if(comName == "")
            {
                return CodeAsComponent;
            }

            StringBuilder builder = new StringBuilder();
            StringBuilder builder1 = new StringBuilder();
            StringBuilder builder2 = new StringBuilder();
            string name = ConfigEditor.fguiTmlpWin.Author;
            if(name == "")
            {
                name = SystemInfo.deviceName;
            }

            CodeUtils.NewLine(builder, 0).Append("--[[\n " + "Author: " + name);
            CodeUtils.NewLine(builder, 0).Append("  "+"Data: " +System.DateTime.Now);
            CodeUtils.NewLine(builder, 0).Append("  "+"Desc: " + "@Class " + comName+"Tpl @"+comName+"模板类\n--]]");
            CodeUtils.NewLine(builder, 0).Append(""+comName+"Tpl = uiclass(\""+comName + "Tpl\"");
            CodeUtils.NewLine(builder, 0).Append("");
            CodeUtils.NewLine(builder, 0).Append(comName +"Tpl.ctor = function(self, ui)");
            CodeUtils.NewLine(builder, 1).Append("self.ui = ui;");
            CodeUtils.NewLine(builder, 1).Append("self.logic = nil;");
            CodeUtils.NewLine(builder, 0).Append("end");
            CodeUtils.NewLine(builder, 0).Append("");
            CodeUtils.NewLine(builder, 0).Append("function "+comName +"Tpl:init(logic, ui)");
            CodeUtils.NewLine(builder, 1).Append("");
            CodeUtils.NewLine(builder, 1).Append("self.ui = ui");
            foreach (var child in childList)
            {
                if(child.是否导出)
                {
                    string childtype = child.typeName.Replace("FairyGUI.", "");
                    if(childtype == "Controller")
                    {
                        CodeUtils.NewLine(builder, 0).Append("--"+child.typeName + child.comDesc);
                        CodeUtils.NewLine(builder, 0).Append("self."+child.comIndex+" = self.ui:GetController(\""+child.comIndex + "\"");
                    }
                    else if(childtype == "Transition")
                    {
                        CodeUtils.NewLine(builder, 0).Append("--"+child.typeName + child.comDesc);
                        CodeUtils.NewLine(builder, 0).Append("self."+child.comIndex+" = self.ui:Transition(\""+child.comIndex + "\"");
                    }
                    else 
                    {
                        CodeUtils.NewLine(builder, 0).Append("--"+child.typeName + child.comDesc);
                        CodeUtils.NewLine(builder, 0).Append("self."+child.comIndex+" = self.ui:GetChild(\""+child.comIndex + "\"");
                    }
                }
            }
            CodeUtils.NewLine(builder, 0).Append("");
            CodeUtils.NewLine(builder, 0).Append("end");
            CodeUtils.NewLine(builder, 0).Append("");

            CodeUtils.NewLine(builder, 0).Append("function "+comName +"Tpl:unCtor()");

            CodeAsComponent = builder.ToString();
            return CodeAsComponent;

        }

        public static string writeFguiLogic(string comName, string type, UIPackage pkg, PackageItem item, List<ChildCom> childList)
        {
            string CodeAsComponent = "";
            customComs.Clear();

            if(comName == "")
            {
                return CodeAsComponent;
            }

            StringBuilder builder = new StringBuilder();
            StringBuilder builder1 = new StringBuilder();
            StringBuilder builder2 = new StringBuilder();
            string name = ConfigEditor.fguiTmlpWin.Author;
            if(name == "")
            {
                name = SystemInfo.deviceName;
            }

            CodeUtils.NewLine(builder, 0).Append("--[[\n " + "Author: " + name);
            CodeUtils.NewLine(builder, 0).Append("  "+"Data: " +System.DateTime.Now);
            CodeUtils.NewLine(builder, 0).Append("  "+"Desc: " + "@Class " + comName+"Logic @"+comName+"逻辑类\n--]]");
            CodeUtils.NewLine(builder, 0).Append(""+comName+"Logic = uiclass(\""+comName + "Logic\", FUIDialogBase");
            CodeUtils.NewLine(builder, 0).Append("");
            // CodeUtils.NewLine(builder, 0).Append(comName +"Logic.ctor = function(self, ui)");
            // CodeUtils.NewLine(builder, 1).Append("self.ui = ui;");
            // CodeUtils.NewLine(builder, 1).Append("self.logic = nil;");
            // CodeUtils.NewLine(builder, 0).Append("end");
            // CodeUtils.NewLine(builder, 0).Append("");
            CodeUtils.NewLine(builder, 0).Append("function "+comName +"Logic:init()");
            CodeUtils.NewLine(builder, 1).Append("");
            CodeUtils.NewLine(builder, 1).Append("self.packageName = \"" + pkg.name +"\"");
            CodeUtils.NewLine(builder, 1).Append("self.resName = \"" + item.name +"\"");
            CodeUtils.NewLine(builder, 1).Append("self.depPack = {}");
            CodeUtils.NewLine(builder, 1).Append("self.param = \"\"");
            CodeUtils.NewLine(builder, 1).Append("self.view = nil");
            CodeUtils.NewLine(builder, 1).Append("self.context = nil");
            CodeUtils.NewLine(builder, 1).Append("---@type" +comName+"Tpl");
            CodeUtils.NewLine(builder, 1).Append("self.tpl = nil");

            CodeUtils.NewLine(builder, 1).Append("self.events = {\n}");

            CodeUtils.NewLine(builder, 1).Append("end");

            CodeUtils.NewLine(builder, 1).Append("");

            CodeUtils.NewLine(builder, 0).Append("function "+comName +"Logic:Register()");
            CodeUtils.NewLine(builder, 1).Append("--初始化ui监听事件");
            CodeUtils.NewLine(builder, 1).Append("self.super.Register(self)");
            CodeUtils.NewLine(builder, 1).Append("end");

            CodeUtils.NewLine(builder, 1).Append("");

            CodeUtils.NewLine(builder, 0).Append("function "+comName +"Logic:initComponent()");
            CodeUtils.NewLine(builder, 0).Append("self.tpl:init(self, self.view)");

            CodeUtils.NewLine(builder, 1).Append("--{{{添加点击监听");
            CodeUtils.NewLine(builder2, 1).Append("--{{{处理点击事件");

            foreach (var child in childList)
            {
                if(child.是否导出)
                {
                    switch (child.typeName)
                    {
                        
                        case "GButton":
                            CodeUtils.NewLine(builder1, 0).Append("self.tpl."+child.comIndex+".onClick:Set(");
                            CodeUtils.NewLine(builder, 0).Append("function() ");
                            CodeUtils.NewLine(builder, 0).Append("self:on_"+child.comIndex+"_click()");
                            CodeUtils.NewLine(builder, 1).Append("end");
                            CodeUtils.NewLine(builder, 1).Append(")");

                            CodeUtils.NewLine(builder, 0).Append("function "+comName + "Logic:on_"+child.comIndex+"_click()");
                            CodeUtils.NewLine(builder, 1).Append("");
                            CodeUtils.NewLine(builder, 1).Append("end");
                            CodeUtils.NewLine(builder, 1).Append("");
                            break;
                    }
                }
            }
            CodeUtils.NewLine(builder, 1).Append(builder1.ToString());

            CodeUtils.NewLine(builder, 1).Append("--}}}");
            CodeUtils.NewLine(builder, 1).Append("self:initData()");
            CodeUtils.NewLine(builder, 0).Append("end");
            CodeUtils.NewLine(builder2, 0).Append("--}}}");

            CodeUtils.NewLine(builder, 0).Append(builder2);
            CodeUtils.NewLine(builder, 0).Append("");
            CodeUtils.NewLine(builder, 0).Append("--界面显示");
            CodeUtils.NewLine(builder, 0).Append("function "+comName +"Logic:StartGames(param)");
            CodeUtils.NewLine(builder, 1).Append("");
            CodeUtils.NewLine(builder, 0).Append("end");

            CodeUtils.NewLine(builder, 0).Append("");

            CodeUtils.NewLine(builder, 0).Append("function "+comName +"Logic:initData()");
            CodeUtils.NewLine(builder, 1).Append("");
            CodeUtils.NewLine(builder, 0).Append("end");

            CodeUtils.NewLine(builder, 0).Append("function "+comName +"Logic:onClose()");
            CodeUtils.NewLine(builder, 1).Append("");
            CodeUtils.NewLine(builder, 0).Append("end");

            CodeUtils.NewLine(builder, 0).Append("");

            foreach (var child in childList)
            {
                if(child.是否导出)
                {
                    switch (child.typeName)
                    {
                        
                        case "GButton":
                            CodeUtils.NewLine(builder1, 0).Append("--"+child.comIndex+"点击事件");
                            CodeUtils.NewLine(builder1, 0).Append("function "+comName + "Logic:"+child.comIndex+"onClick()");
                            CodeUtils.NewLine(builder1, 1).Append("");
                            CodeUtils.NewLine(builder1, 1).Append("end");
                            break;
                    }
                }
            }

            CodeUtils.NewLine(builder, 0).Append("return "+comName +"Logic");

            CodeAsComponent = builder.ToString();
            return CodeAsComponent;

        }
    }
}

#endif
