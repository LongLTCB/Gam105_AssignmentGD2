using static UnityEditor.Progress;
using UnityEngine;
using UnityEngine.UI;

public class ItemSlot : MonoBehaviour
{
    public Image ItemIcon;  // Biến chứa UI hình ảnh item
    private Item item;      // Biến chứa thông tin item

    // Gán item vào slot
    public void SetItem(Item newItem)
    {
        item = newItem; // Lưu item vào slot

        if (item != null)
        {
            ItemIcon.sprite = item.icon; // Gán hình ảnh của item vào UI
            ItemIcon.enabled = true;
        }
        else
        {
            ItemIcon.sprite = null;
            ItemIcon.enabled = false;
        }
    }
}

