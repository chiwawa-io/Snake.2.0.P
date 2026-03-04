using System.Collections.Generic;
using UnityEngine;

namespace Luxodd.Game.Scripts.Missions
{
    [CreateAssetMenu(menuName = "Create/Items/Items Data Base", fileName = "ItemsDataBase", order = 0)]
    public class ItemsDataBase : ScriptableObject
    {
        [SerializeField] private List<ItemDataDescriptor> _itemDataDescriptors = new List<ItemDataDescriptor>();

        public ItemDataDescriptor this[ItemType itemType]
        {
            get
            {
                var result = _itemDataDescriptors.Find(item => item.ItemType == itemType);
                return result;
            }
        }
    }

    [System.Serializable]
    public class ItemDataDescriptor
    {
        [field: SerializeField] public ItemType ItemType { get; private set; }
        [field: SerializeField] public string ItemName { get; private set; }
        [field: SerializeField] public string SpriteKey { get; private set; }
        
    }
}
