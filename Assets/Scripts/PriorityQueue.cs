using System.Collections.Generic;

public class PriorityQueue<T>
{
    private List<T> _data;
    private IComparer<T> _comparer;
    public T Peek() => _data[0];
    public int Count => _data.Count;
    
    public PriorityQueue(IComparer<T> comparer)
    {
        _data = new List<T>();
        _comparer = comparer;
    }
    
    public void Enqueue(T item)
    {
        _data.Add(item);
        EnqueueSort(_data.Count);
    }
    
    private void EnqueueSort(int idx)
    {
        if (idx <= 1)
            return;
        
        int compare = _comparer.Compare(GetValueAt(idx), GetValueAt(idx / 2));

        if (compare <= 0)
            return;
        
        Swap(idx, idx / 2);
        
        EnqueueSort(idx / 2);
    }
    
    public T Dequeue()
    {
        T item = _data[0];
        
        _data[0] = _data[_data.Count - 1];
        
        _data.RemoveAt(_data.Count - 1);
        
        DequeueSort(1);
        
        return item;
    }
    
    private void DequeueSort(int idx)
    {
        if (idx * 2 > _data.Count)
            return;
        
        if(idx * 2 + 1 > _data.Count)
        {
            int compare = _comparer.Compare(GetValueAt(idx), GetValueAt(idx * 2));
            if (compare < 0)
                Swap(idx, idx * 2);
            return;
        }
        
        int compare1 = _comparer.Compare(GetValueAt(idx), GetValueAt(idx * 2));
        int compare2 = _comparer.Compare(GetValueAt(idx), GetValueAt(idx * 2 + 1));
        
        if (compare1 > 0 && compare2 > 0)
            return;
        
        int swapCompare = _comparer.Compare(GetValueAt(idx * 2), GetValueAt(idx * 2 + 1));
        
        int swapIdx = swapCompare > 0 ? idx * 2 : idx * 2 + 1;
        
        Swap(idx, swapIdx);
        
        DequeueSort(swapIdx);
    }
    
    private void Swap(int idx1, int idx2)
    {
        (_data[idx1 - 1], _data[idx2 - 1]) = (_data[idx2 - 1], _data[idx1 - 1]);
    }
    
    private T GetValueAt(int idx) => _data[idx - 1];
    
}