using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using FairyGUI;
using XiahExcel;

public class FguiUtils
{
    public static int CreateCount = 0;
    public static int DeleteCount = 0;

    public static string GetFguiTypeFromObjectType(ObjectType type)
    {
        return "G" + type.ToString();
    }

    public static List<ChildCom> getComChilds(PackageItem childpkg, ref List<ChildCom> childComs)
    {
        if(childpkg.name == null)
        {
            Debug.LogError("错误");
            return null;
        }

        GObject obj = UIPackage.CreateObject(ConfigEditor.fguiTmlpWin.curPkg.name, childpkg.name);

        //立即dispose
        obj.Dispose();

        if(childComs == null)
            childComs = new List<ChildCom>();

        if(childComs.Count!=0)
            childComs.Clear();
        

        GObject[] childArr;
        if (obj is GComponent)
        {
            childArr = (obj as GComponent).GetChildren();

            List<Controller> controllerList = (obj as GComponent).Controllers;
            List<Transition> transitionsList = (obj as GComponent).Transitions;

            foreach (var transition in transitionsList)
            {
                ChildCom childCom = new ChildCom();
                childCom.typeName = "Transition";
                childCom.comDesc = "动效";
                childCom.是否导出 = true;
                childCom.comIndex = transition.name;
                childCom.getTypeDesc = "GetTransition";

                childCom.pkgitem = childpkg;

                if(isAddComChilds(transition.name))
                {
                    childComs.Add(childCom);
                }
            }

            foreach (var controller in controllerList)
            {
                ChildCom childCom = new ChildCom();
                childCom.typeName = "Controller";
                childCom.comDesc = "控制器";
                childCom.是否导出 = true;
                childCom.comIndex = controller.name;
                childCom.getTypeDesc = "GetController";

                childCom.pkgitem = childpkg;

                if(isAddComChilds(controller.name))
                {
                    childComs.Add(childCom);
                }
            }

            foreach( var child in childArr)
            {
                Type t = child.GetType();

                PackageItem comItem = UIPackage.GetItemByURL(child.resourceURL);

                if(child is GList){}
                else if(child is GComponent)
                {
                    
                }
                ChildCom childCom = new ChildCom();
                childCom.typeName = t.ToString().Replace("FairyGUI.", "");

                if(ConfigEditor.fguiTmlpWin.excludeTypes.Contains(childCom.typeName))
                {
                    childCom.comDesc = childCom.typeName.Replace("FairyGUI.", "");
                    childCom.是否导出 = false;
                }
                else
                {
                    childCom.comDesc = "";
                }

                if(ConfigEditor.fguiTmlpWin.extenstionBindTypes.Contains(childCom.typeName))
                {
                    if(childCom.typeName == "GList")
                    {
                        if ((child as GList).defaultItem != null)
                        {
                            PackageItem pkg = UIPackage.GetItemByURL((child as GList).defaultItem);

                            childCom.comDesc = pkg.name;
                        }
                        else
                        {
                            childCom.comDesc = null;
                        }
                        
                    }
                    
                }

                childCom.comIndex = child.name;
                if(childCom.comDesc == null)
                    childCom.comDesc = childCom.typeName;

                childCom.pkgitem = comItem;

                if(isAddComChilds(child.name))
                {
                    childComs.Add(childCom);
                }
            }
            
        }
        else
        {
            return null;
        }

        foreach (var gObject in childArr.ToArray())
        {
            if(gObject.displayObject != null)
                gObject.displayObject.Dispose();

            gObject.Dispose();
        }
        return childComs;


    }

    private static bool isAddComChilds(string name)
    {
        if(ConfigEditor.fguiTmlpWin.IsDeriveNoNameNode)
        {
            return true;
        }

        for (int k = 0; k < ConfigEditor.fguiTmlpWin.FGUIComments.Length; k++)
        {
            if(name.IndexOf(ConfigEditor.fguiTmlpWin.FGUIComments[k]) >= 0)
            {
                return true;
            }
        }
        return false;
    }

    public static Dictionary<string, ChildCom> getAllPkgChilds(ref List<ChildCom> allPkgChildComs)
    {
        List<PackageItem> items = ConfigEditor.fguiTmlpWin.curPkg.GetItems();

        if(allPkgChildComs == null)
            allPkgChildComs = new List<ChildCom>();

        if(allPkgChildComs.Count != 0)
            allPkgChildComs.Clear();
        
        foreach (var pkgitem in items)
        {
            if(pkgitem.name == null && pkgitem.type == PackageItemType.Atlas)
            {
                continue;
            }

            List<ChildCom> childPkgList = getComChilds(pkgitem, ref ConfigEditor.fguiTmlpWin.childComs);

            if(childPkgList != null)
            {
                allPkgChildComs = allPkgChildComs.Union(childPkgList).ToList();
            }
        }

        Dictionary<string, ChildCom> bindPkgChilds = new Dictionary<string, ChildCom>();

        foreach (var child in allPkgChildComs)
        {
            if(ConfigEditor.fguiTmlpWin.extenstionBindTypes.Contains(child.typeName))
            {
                if(child.comDesc != null)
                {
                    if(!bindPkgChilds.ContainsKey(child.comDesc))
                        bindPkgChilds.Add(child.comDesc, child);
                }
            }
        }

        allPkgChildComs.Clear();
        return bindPkgChilds;
    }

}
