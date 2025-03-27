using UnityEngine;
using static UnityEditor.Progress;

public class InventoryManager : MonoBehaviour
{
    public ItemSlot[] itemSlots; // Mảng các ô chứa item

    public void AddItem(Item newItem)
    {
        foreach (ItemSlot slot in itemSlots)
        {
            if (slot != null)
            {
                slot.SetItem(newItem);
                break;
            }
        }
    }
}