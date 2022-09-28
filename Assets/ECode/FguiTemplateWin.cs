using System.Security.Cryptography;
using System.Net.Mime;
using System.Numerics;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using System.Linq;
using FairyGUI;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;


namespace XiahExcel
{
    public class FguiTemplateWin
    {
        private const string FGUIFilePATH = "FguiFilePath";
        private const string FGUIFileFolder = "FguiFileFolder";
        private const string OUTPUTFOLDER = "FguiOutputFolder";

        private bool isCompliing = false;

        [EnumToggleButtons] public ProjectEnum 当前工程 = ProjectEnum.测试项目;

        [OnInspectorInit]
        private void onUpdate()
        {
            if(EditorApplication.isCompiling)
            {
                isCompliing = true;
            }
            else
            {
                isCompliing = false;
            }
        }

        private void onFguiFilePathChange()
        {
            EditorPrefs.SetString(FGUIFilePATH, fguiFilePath);

            if(fguiFilePath != null || fguiFilePath != "")
            {
                DirectoryInfo dir = Directory.GetParent(fguiFilePath);
            }

            FreshFgui();
        }

        private void onFguiTplOutputPathChange()
        {
            EditorPrefs.SetString(OUTPUTFOLDER, fguiOutPutPath);
        }

        private void onFguiFolderChange()
        {
            EditorPrefs.SetString(FGUIFileFolder, fguiFolderName);
        }


        [PropertyTooltip("请选择fgui ui 的bytes文件")]
        [FilePath(AbsolutePath = true, Extensions = "bytes")]
        [OnValueChanged("onFguiFilePathChange")]
        [PropertySpace(SpaceBefore = 0, SpaceAfter = 20)]
        [PropertyOrder(0)]
        public string fguiFilePath;

        [HideInInspector]
        [OnValueChanged("onFguiFolderChange")]
        [PropertyTooltip("请选择fgui文件夹名称")]
        public string fguiFolderName;

        [PropertyTooltip("请选择模板导出的文件夹")]
        [FolderPath(AbsolutePath = true)]
        [OnValueChanged("onFguiTplOutputPathChange")]
        [PropertySpace(SpaceBefore = 0, SpaceAfter = 20)]
        [PropertyOrder(0)]
        public string fguiOutPutPath;

        [HideInInspector] public UIPackage curPkg;

        ///
        ///读取pkg
        ///
        [ButtonGroup]
        [Button(ButtonSizes.Medium), GUIColor(0, 1, 0)]
        private void FreshFgui()
        {
            if(fguiFilePath == "")
            {
                UnityEngine.Debug.Log("xml不能为空");
                return;
            }

            string fileName = Path.GetFileNameWithoutExtension(fguiFilePath).Replace("_fui", "");

            curPkg = UIPackage.AddPackage("Assets/GameResources/FUI/" + fileName);

            List<string> depkgList = new List<string>();
            UIPackage depPkg;
            foreach (var deplist in curPkg.dependencies)
            {
                foreach (var name in deplist)
                {
                    if(name.Key == "name")
                    {
                        depkgList.Add(name.Value);
                        try{
                            UIPackage.AddPackage("Assets/GameResources/FUI/" + name.Value);
                        }
                        catch(Exception ex)
                        {
                            UnityEngine.Debug.Log($"{fileName}包没找到对应的依赖包{name.Value},请检查");
                        }
                    }
                }
            }

            depkgList = null;
            List<PackageItem> items = curPkg.GetItems();
            List<ObjectType> ExportTypes = new List<ObjectType>()
            {
                ObjectType.Component, ObjectType.Button
            };

            if(Items != null)
            {
                Items.Clear();
            }
            else
            {
                Items = new List<UIItem>();
            }

            string pinYingName;
            foreach (var item in items)
            {
                if(ExportTypes.Contains(item.objectType))
                {
                    pinYingName = item.name;

                    UIItem uiItem = new UIItem();
                    uiItem.uiName = item.name;
                    uiItem.pinYingName = pinYingName;
                    uiItem.item = item;
                    Items.Add(uiItem);
                }
            }

            ItemsBackup = new List<UIItem>(Items);
            foreach (var item in Items)
            {
                item.items = Items;
            }

        }

        [OnInspectorDispose]
        void onDispose()
        {
            if(curPkg != null)
            {
                UIPackage.RemoveAllPackages();
                curPkg = null;
            }
        }

        [HideInInspector] public bool NoneSelectCom = true;

        [DisableIf("NoneSelectCom")]
        [ButtonGroup]
        [Button(ButtonSizes.Medium), GUIColor(0, 1, 0)]
        public void 更新模板()
        {
            if(curItem == null)
            {
                UnityEngine.Debug.LogError("请选择一个控件");
                return;
            }

            if(curPkg == null)
            {
                UnityEngine.Debug.LogError("请选择一个FGUI文件");
                return;
            }

            string comName = curItem.pinYingName;
            string type = FguiUtils.GetFguiTypeFromObjectType(curItem.item.objectType);

            Dictionary<string, ChildCom> childList = FguiUtils.getAllPkgChilds(ref allPkgChildComs);
            switch(LanguageEnumField)
            {
                case LanguageEnum.Lua:
                    CodeAsComponent = 
                        GenLuaCodeByFguiXml.writeFguiTpl(comName, type, curPkg, curItem.item, selectChildComs);
                    CodeAsCustomComponent =  
                        GenLuaCodeByFguiXml.writeFguiLogic(comName, type, curPkg, curItem.item, selectChildComs);

                    break;
                case LanguageEnum.CSharp:
                    break;

                default:
                    UnityEngine.Debug.LogError("错误的语言类型");
                    break;
            }
        }

        [DisableIf("NoneSelectCom")]
        [ButtonGroup]
        [Button(ButtonSizes.Medium), GUIColor(0, 1, 0)]
        public void 导出选中的相关文件()
        {
            if(curItem != null)
            {
                if(!Directory.Exists(fguiOutPutPath))
                {
                    Directory.CreateDirectory(fguiOutPutPath);
                }

                AssetDatabase.Refresh();
            }
            else
            {
                UnityEngine.Debug.LogError("请选择一个控件");
            }
        }

        #region 设置

        [PropertyTooltip("设置生成的类型")][ToggleGroup("Setting")]
        public bool Setting = true;
        [ToggleGroup("Setting")][Title("生成语言")][EnumToggleButtons]
        public LanguageEnum LanguageEnumField = LanguageEnum.Lua;

        public enum LanguageEnum
        {
            Lua,
            CSharp
        }

        [PropertyTooltip("不生成代码类型")][ToggleGroup("Setting")][Title("不生成代码类型")]
        public List<string> excludeTypes = new List<string>{"GGroup"};

        [PropertyTooltip("生成自定义代码的组件")][ToggleGroup("Setting")][Title("生成自定义代码的组件")]
        public List<string> extenstionBindTypes = new List<string>{"GComponent"};

        [TableColumnWidth(100, Resizable = false)][ToggleGroup("Setting")][Title("是否导出未命名节点")]
        public bool IsDeriveNoNameNode = false;

        [PropertyTooltip("节点命名格式")][ToggleGroup("Setting")][Title("节点命名格式")]
        public string[] FGUIComments = new string[]
        {
            "txt_","loader_","btn_","graph_","list_","loader3d_","com_",
            "group_","ctrl_","img_","scrollBar_","slider_","textInput_"
        };

        [PropertyTooltip("作者名")][ToggleGroup("Setting")][Title("作者名")]
        public string Author = "default";

        #endregion


        #region 列表
        [DisableIf("isCompliing")]
        [PropertyTooltip("选中FGUI包中的所有控件列表")]
        [HorizontalGroup("List")]
        [TableList(ShowPaging = true, NumberOfItemsPerPage = 10, AlwaysExpanded = true)]
        [Searchable]
        public List<UIItem> Items;

        private List<UIItem> ItemsBackup;

        [DisableIf("isCompliing")]
        [PropertyTooltip("选中控件的所有子控件")]
        [HorizontalGroup("List")]
        [TableList(ShowPaging = true, NumberOfItemsPerPage = 10, AlwaysExpanded = true)]
        [Searchable]
        public List<ChildCom> selectChildComs;

        public UIItem curItem
        {
            get
            {
                foreach (var item in Items)
                {
                    if(item.Enable)
                    {
                        return item;
                    }
                }
                return null;
            }
        }

        [HideInInspector] List<ChildCom> allPkgChildComs;
        [HideInInspector] public List<ChildCom> childComs;
        
        #endregion

        public static Color BackgroundColor = new Color(0.118f, 0.118f, 0.118f, 1f);

        public static Color TextColor = new Color(0.863f, 0.863f, 0.863f, 1f); 

        private bool showComponent = true;

        private string CodeAsComponent = "";

        private string highlightedCode = "";

        private string highlightedCodeAsComponent = "";

        private string ComponentName = "";

        private GUIStyle codeTextStyle;
        private UnityEngine.Vector2 scrollPosition;


        [OnInspectorGUI()]
        [DisableIf("isCompliing")]
        [FoldoutGroup("UI模板代码")]
        public void Draw()
        {
            GUILayout.Space(12f);
            GUILayout.Label("生成的代码", SirenixGUIStyles.BoldTitle, new GUILayoutOption[0]);
            Rect rect2 = SirenixEditorGUI.BeginToolbarBox(new GUILayoutOption[0]);

            SirenixEditorGUI.DrawSolidRect(rect2.HorizontalPadding(1f), BackgroundColor, true);
            SirenixEditorGUI.BeginToolbarBoxHeader(22f);

            GUILayout.FlexibleSpace();
            if(SirenixEditorGUI.ToolbarButton("复制代码", false))
            {
                Clipboard.Copy<string>(CodeAsComponent);
            }

            if(CodeAsComponent != null && SirenixEditorGUI.ToolbarButton("保存代码", false))
            {
                string text = "";
                if(LanguageEnumField == LanguageEnum.Lua)
                {
                    text = EditorUtility.SaveFilePanelInProject("Create FGUI Tempiate File",
                        curItem.pinYingName + "Tpl.lua", "lua", "选择保存的文件夹", fguiOutPutPath);
                }
                else
                {
                    text = EditorUtility.SaveFilePanelInProject("Create FGUI Tempiate File",
                        "UI_"+curItem.pinYingName + "_tpl.cs", "cs", "选择保存的文件夹", fguiOutPutPath);
                }

                if(!string.IsNullOrEmpty(text))
                {
                    File.WriteAllText(text, CodeAsComponent);
                    AssetDatabase.Refresh();
                }

                GUIUtility.ExitGUI();
            }

            SirenixEditorGUI.EndToolbarBoxHeader();
            if(codeTextStyle == null)
            {
                codeTextStyle = new GUIStyle(SirenixGUIStyles.MultiLineLabel);
                codeTextStyle.normal.textColor = TextColor;
                codeTextStyle.active.textColor = TextColor;
                codeTextStyle.focused.textColor = TextColor;
                codeTextStyle.wordWrap = false;
            }

            try{
                if(LanguageEnumField == LanguageEnum.CSharp)
                {
                    // highlightedCodeAsComponent = SyntaxHighlighter.Parse(CodeAsComponent);
                }
                else
                {
                    highlightedCodeAsComponent = CodeAsComponent;
                }
            }
            catch(Exception ex2)
            {
                UnityEngine.Debug.LogException(ex2);
                highlightedCodeAsComponent = CodeAsComponent;
            }

            GUIContent guicontent = GUIHelper.TempContent(this.highlightedCodeAsComponent);
            UnityEngine.Vector2 vector = codeTextStyle.CalcSize(guicontent);
            GUILayout.BeginHorizontal(new GUILayoutOption[0]);
            GUILayout.Space(-3f);
            GUILayout.BeginVertical(new GUILayoutOption[0]);
            Rect rect3 = GUILayoutUtility.GetRect(vector.x +50f, vector.y).AddXMin(4f).AddY(2f);
            GUI.Label(rect3, guicontent, codeTextStyle);

            GUILayout.EndVertical();
            GUILayout.Space(-3f);
            GUILayout.EndHorizontal();
            GUILayout.Space(-3f);
            SirenixEditorGUI.EndToolbarBox();
        }

        private string CodeAsCustomComponent = "";
        private string highlightedCodeAsCustomComponent = "";

        [OnInspectorGUI]
        [DisableIf("isCompliing")]
        [FoldoutGroup("UI模板Custom代码")]

        public void DrawCustom()
        {
            GUILayout.Space(12f);
            GUILayout.Label("生成的代码", SirenixGUIStyles.BoldTitle, new GUILayoutOption[0]);
            Rect rect2 = SirenixEditorGUI.BeginToolbarBox(new GUILayoutOption[0]);

            SirenixEditorGUI.DrawSolidRect(rect2.HorizontalPadding(1f), BackgroundColor, true);
            SirenixEditorGUI.BeginToolbarBoxHeader(22f);

            GUILayout.FlexibleSpace();
            if(SirenixEditorGUI.ToolbarButton("复制代码", false))
            {
                Clipboard.Copy<string>(CodeAsCustomComponent);
            }

            if(CodeAsCustomComponent != null && SirenixEditorGUI.ToolbarButton("保存代码", false))
            {
                string text = "";
                if(LanguageEnumField == LanguageEnum.Lua)
                {
                    text = EditorUtility.SaveFilePanelInProject("Create FGUI Logic File",
                        curItem.pinYingName + "Logic.lua", "lua", "选择保存的文件夹", fguiOutPutPath);
                }
                else
                {
                    text = EditorUtility.SaveFilePanelInProject("Create FGUI Tempiate File",
                        "UI_"+curItem.pinYingName + "_logic.cs", "cs", "选择保存的文件夹", fguiOutPutPath);
                }

                if(!string.IsNullOrEmpty(text))
                {
                    File.WriteAllText(text, CodeAsCustomComponent);
                    AssetDatabase.Refresh();
                }

                GUIUtility.ExitGUI();
            }

            SirenixEditorGUI.EndToolbarBoxHeader();
            if(codeTextStyle == null)
            {
                codeTextStyle = new GUIStyle(SirenixGUIStyles.MultiLineLabel);
                codeTextStyle.normal.textColor = TextColor;
                codeTextStyle.active.textColor = TextColor;
                codeTextStyle.focused.textColor = TextColor;
                codeTextStyle.wordWrap = false;
            }

            try{
                if(LanguageEnumField == LanguageEnum.CSharp)
                {
                    // highlightedCodeAsCustomComponent = SyntaxHighlighter.Parse(CodeAsCustomComponent);
                }
                else
                {
                    highlightedCodeAsCustomComponent = CodeAsCustomComponent;
                }
            }
            catch(Exception ex2)
            {
                UnityEngine.Debug.LogException(ex2);
                highlightedCodeAsCustomComponent = CodeAsCustomComponent;
            }

            GUIContent guicontent = GUIHelper.TempContent(this.highlightedCodeAsCustomComponent);
            UnityEngine.Vector2 vector = codeTextStyle.CalcSize(guicontent);
            GUILayout.BeginHorizontal(new GUILayoutOption[0]);
            GUILayout.Space(-3f);
            GUILayout.BeginVertical(new GUILayoutOption[0]);
            Rect rect3 = GUILayoutUtility.GetRect(vector.x +50f, vector.y).AddXMin(4f).AddY(2f);
            GUI.Label(rect3, guicontent, codeTextStyle);

            GUILayout.EndVertical();
            GUILayout.Space(-3f);
            GUILayout.EndHorizontal();
            GUILayout.Space(-3f);
            SirenixEditorGUI.EndToolbarBox();
        }

        public void SaveCache()
        {
            string filepath = Application.persistentDataPath + Path.DirectorySeparatorChar + "EditorCache";

            if(Items == null)
            {
                return;
            }
            byte[] bytes = SerializationUtility.SerializeValue(Items, DataFormat.Binary);
            File.WriteAllBytes(filepath, bytes);
        }

        public void ReadCache()
        {
            string filepath = Application.persistentDataPath + Path.DirectorySeparatorChar + "EditorCache";
            if(!File.Exists(filepath))
            {
                return;
            }
            byte[] bytes = File.ReadAllBytes(filepath);

            Items = SerializationUtility.DeserializeValue<List<UIItem>>(bytes, DataFormat.Binary);
        }


    }

    [Serializable]
    public class ChildCom
    {
        [TableColumnWidth(100, Resizable = false)]
        public bool 是否导出 = true;

        [HideInInspector]
        [TableColumnWidth(100, Resizable = false)]
        public bool 注册自定义 = false;

        [PropertyTooltip("类型")]
        public string typeName = "";
        [PropertyTooltip("名称")]
        public string comIndex = "";
        [PropertyTooltip("获取类型")]
        public string getTypeDesc = "";
        [PropertyTooltip("描述")]
        public string comDesc = "";
        [HideInInspector]
        public string customVarname = "";

        [HideInInspector]
        public string customClsName = "";
        [HideInInspector]
        public PackageItem pkgitem;
    }

    [Serializable]
    public class UIItem
    {
        [OnValueChanged("OnEnableChange")]
        public bool Enable = false;

        private FguiTemplateWin fguiTmlpWin;

        void OnEnableChange()
        {
            if(fguiTmlpWin == null)
            {
                fguiTmlpWin = ConfigEditor.fguiTmlpWin;
            }

            if(Enable)
            {
                foreach (var item in items)
                {
                    if(item.uiName != uiName && item.Enable)
                    {
                        item.Enable = false;
                    }
                }

                if(ConfigEditor.fguiTmlpWin.selectChildComs == null)
                    ConfigEditor.fguiTmlpWin.selectChildComs = new List<ChildCom>();

                if(ConfigEditor.fguiTmlpWin.selectChildComs.Count != 0)
                    ConfigEditor.fguiTmlpWin.selectChildComs.Clear();


                List<ChildCom> childComsList = FguiUtils.getComChilds(item, ref fguiTmlpWin.childComs);
                if(childComsList == null)
                    childComsList = new List<ChildCom>();

                
                fguiTmlpWin.selectChildComs = fguiTmlpWin.selectChildComs.Concat(childComsList).ToList();
                fguiTmlpWin.NoneSelectCom = false;
                fguiTmlpWin.更新模板();

                // fguiTmlpWin.SaveCache();
            }
            else
            {
                fguiTmlpWin.NoneSelectCom = true;
            }
        }

        [PropertyTooltip("控件的ui名")]
        public string uiName = "";

        [OnValueChanged("OnEnableChange")]
        [PropertyTooltip("控件的ui名转拼音")]
        public string pinYingName = "";

        [HideInInspector]
        public PackageItem item;

        [HideInInspector]
        public List<UIItem> items;
    }
}
