#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace SupersonicWisdomSDK
{
    [ExecuteInEditMode]
    public class SwGameObjectLogo : MonoBehaviour
    {
#if UNITY_EDITOR
        private static Texture2D _logo;

        private void OnEnable ()
        {
            EditorApplication.hierarchyWindowItemOnGUI += HierarchyWindowCallback;
        }

        private void OnDisable ()
        {
            if (EditorApplication.hierarchyWindowItemOnGUI != null)
                EditorApplication.hierarchyWindowItemOnGUI -= HierarchyWindowCallback;
        }

        private static void HierarchyWindowCallback(int instanceId, Rect selectionRect)
        {
            var gameObject = (GameObject)EditorUtility.InstanceIDToObject(instanceId);

            if (gameObject != null && gameObject.GetComponent<SwCoreMonoBehaviour>() != null)
            {
                Graphics.DrawTexture(new Rect(GUILayoutUtility.GetLastRect().width - selectionRect.height - 5, selectionRect.y, selectionRect.height, selectionRect.height), Logo);
            }
        }

        public static Texture2D Logo
        {
            get
            {
                if (_logo == null)
                {
                    _logo = (Texture2D)AssetDatabase.LoadAssetAtPath(SwUtils.WhereIs(LogoFileName), typeof(Texture2D));
                }

                return _logo;
            }
        }

        private const string LogoFileName = "SwLogo.png";
#endif
    }
}