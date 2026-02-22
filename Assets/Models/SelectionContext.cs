using System.Collections.Generic;

[System.Serializable]
public class SelectionContext
{
    public string prompt;
    public List<SelectedObjectData> selection;
    public SelectionBounds bounds;
}
