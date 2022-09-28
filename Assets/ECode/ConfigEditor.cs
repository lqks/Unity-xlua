using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;

#if UNITY_EDITOR
namespace XiahExcel
{
    public class ConfigEditor : OdinMenuEditorWindow
    {
        [MenuItem("工具/FGUI生成模板")]
        private static void OpenWindow()
        {
            var window = GetWindow<ConfigEditor>();
            window.position = GUIHelper.GetEditorWindowRect().AlignCenter(1000, 600);
        }

        public static FguiTemplateWin fguiTmlpWin = new FguiTemplateWin();

        protected override OdinMenuTree BuildMenuTree()
        {
            OdinMenuTree tree = new OdinMenuTree(supportsMultiSelect:true)
            {
                {"fgui模板", fguiTmlpWin, EditorIcons.Flag},
            };
            return tree;
        }
    }
}

#endif