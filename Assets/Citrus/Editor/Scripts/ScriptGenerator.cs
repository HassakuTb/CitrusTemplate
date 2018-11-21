using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Citrus.Editor {

    /// <summary>
    /// スクリプト生成ウィンドウ
    /// </summary>
    public class ScriptGenerator : ScriptableWizard {

        private const string templateFolderPath = "Assets/Citrus/Editor/ScriptTemplates/";
        private const string menuPath = "Assets/Create/Citrus/Scripts";
        
        private class ScriptItem {
            public ScriptEnum script;
            public string templatePath;
            public string defaultFilename;
        }

        private static Dictionary<ScriptEnum, ScriptItem> itemDictionary;

        //  生成可能なスクリプトを列挙
        private enum ScriptEnum {
            MonoBehaviour,
            SingletonMonoBehaviour,
        }

        private static IEnumerable<ScriptItem> EnumerateScriptItem()
        {
            yield return new ScriptItem
            {
                script = ScriptEnum.MonoBehaviour,
                templatePath = templateFolderPath + "MonoBehaviour.txt",
                defaultFilename = "NewMonoBehaviour",
            };
            yield return new ScriptItem
            {
                script = ScriptEnum.SingletonMonoBehaviour,
                templatePath = templateFolderPath + "SingletonMonoBehaviour.txt",
                defaultFilename = "NewSingletonMonoBehaviour",
            };
        }
        
        private static void CreateScriptDictionary()
        {
            itemDictionary = EnumerateScriptItem().ToDictionary(x => x.script);
        }

        [SerializeField] private ScriptEnum script;
        [SerializeField] private string namespaceString;
        [SerializeField] private string fileName;

        private ScriptEnum previousScript;
        private string path;

        [MenuItem(menuPath, false, 0)]
        private static void OpenGenerateScriptWindow()
        {
            if (itemDictionary == null) CreateScriptDictionary();

            ScriptGenerator window = DisplayWizard<ScriptGenerator>("Script Generator");
            
            var obj = Selection.activeObject;
            if (obj == null) window.path = "Assets";
            else window.path = AssetDatabase.GetAssetPath(obj.GetInstanceID());
            var paths = window.path.Split('/').SkipWhile(x => x != "Scripts").Skip(1);
            string namespaceStr = PlayerSettings.productName + "." + string.Join(".", paths);

            window.script = ScriptEnum.MonoBehaviour;
            window.previousScript = ScriptEnum.MonoBehaviour;
            window.namespaceString = namespaceStr;
            window.fileName = itemDictionary[window.script].defaultFilename;
        }

        private void OnWizardUpdate()
        {
            if(script != previousScript)
            {
                fileName = itemDictionary[script].defaultFilename;
                previousScript = script;
            }
        }

        private void OnWizardCreate()
        {
            string template;
            using(StreamReader reader = new StreamReader(itemDictionary[script].templatePath, Encoding.UTF8))
            {
                template = reader.ReadToEnd();
            }

            while (namespaceString.EndsWith(".")) namespaceString = namespaceString.Substring(0, namespaceString.Length - 1);

            var paths = path.Split('/').SkipWhile(x => x != "Scripts").Skip(1);
            string componentMenuPath = PlayerSettings.productName + "/" + string.Join("/", paths);

            string code = template
                .Replace("#SCRIPTNAME#", fileName)
                .Replace("#COMPONENTMENUPATH#", componentMenuPath)
                .Replace("#NAMESPACE#", namespaceString);

            File.WriteAllText(path + "/" + fileName + ".cs", code, Encoding.UTF8);
            AssetDatabase.Refresh();
        }
    }
}
