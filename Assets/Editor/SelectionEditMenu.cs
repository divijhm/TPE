using UnityEditor;
using UnityEngine;

public class SelectionEditMenu
{
    [MenuItem("DAMN/Selection Edit")]
    static void SendSelection()
    {
        var context = SelectionContextBuilder.Build();

        Debug.Log(JsonUtility.ToJson(
            new Wrapper(context), true));
    }

    class Wrapper
    {
        public object selection;
        public Wrapper(object s) { selection = s; }
    }
}