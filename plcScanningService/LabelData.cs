using System;
using System.Collections;

[Serializable]
public class LabelData
{
    public string name;
    public SortedList data;

    public LabelData() {
        data = new SortedList();
    }

    public void add(DateTime dt, long label) {
        data.Add(label, dt);
    }
}
