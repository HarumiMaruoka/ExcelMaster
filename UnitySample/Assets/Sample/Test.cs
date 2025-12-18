using UnityEngine;
using UnityEngine.InputSystem;

public class Test : MonoBehaviour
{
    [SerializeField]
    private int itemId;

    private void Update()
    {
        if (Keyboard.current.uKey.wasPressedThisFrame)
        {
            var path = "Assets/ExcelMaster/Data/Binary/Item.mmdb";
            var binary = System.IO.File.ReadAllBytes(path);

            // MemoryDatabaseをバイナリから作成
            var memoryDatabase = new MasterMemory.MemoryDatabase(binary);
            // テーブルからデータを検索
            var item = memoryDatabase.ItemTable.FindById(itemId);
            Debug.Log(item.Name);
        }
    }
}