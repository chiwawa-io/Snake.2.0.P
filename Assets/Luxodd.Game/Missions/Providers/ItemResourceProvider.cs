using System;
using System.Collections.Generic;
using UnityEngine;

namespace Luxodd.Game.Scripts.Missions
{
    [CreateAssetMenu(menuName = "Create/Items/Item Resource Provider", fileName = "ItemResourceProvider", order = 0)]
    public class ItemResourceProvider : ScriptableObject
    {
        [SerializeField] private string _pathPrefix = "Items/";
        
        [SerializeField] private List<ItemResourceData> _items = new List<ItemResourceData>();

        public Sprite ProvideSprite(string spriteKey)
        {
            var itemData = _items.Find(x => x.SpriteKey == spriteKey);
            var path = _pathPrefix + itemData.SpritePath;
            var sprite = Resources.Load<Sprite>(path);
            return sprite;
        }
    }

    [Serializable]
    public class ItemResourceData
    {
        [field: SerializeField] public ItemType ItemType { get; private set; }
        [field: SerializeField] public string SpritePath { get; private set; }
        [field: SerializeField] public string SpriteKey { get; private set; }
    }
}
