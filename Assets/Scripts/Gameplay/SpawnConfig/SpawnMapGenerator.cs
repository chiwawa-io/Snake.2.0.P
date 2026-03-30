using Core.Enums;
using Services.RNG;
using UnityEngine;

namespace Gameplay.SpawnConfig
{
    public class SpawnMapGenerator
    {
        private const int FoodSpacing = 5;
        private const int PreciousSpacing = 10;
        private const int PowerUpPadding = 3;

        private readonly IRngService _rngService;
        
        public SpawnMapGenerator(IRngService rngService)
        {
            _rngService = rngService;
        }

        public RuntimeSpawnMap Generate(Vector2Int boardSize, bool isSbSession)
        {
            var map = new RuntimeSpawnMap();

            int xBound = boardSize.x / 2;
            int yBound = boardSize.y / 2;

            for (int x = -xBound + 2; x <= xBound - 2; x += FoodSpacing)
            {
                for (int y = -yBound + 2; y <= yBound - 2; y += FoodSpacing)
                {
                    map.AddPoint(ItemType.Food, new Vector2Int(x, y));
                }
            }

            for (int x = -xBound + 4; x <= xBound - 4; x += PreciousSpacing)
            {
                for (int y = -yBound + 4; y <= yBound - 4; y += PreciousSpacing)
                {
                    map.AddPoint(ItemType.PreciousFood, new Vector2Int(x, y));
                }
            }
            
            var speedModifierType = isSbSession ? ItemType.SlowDown : ItemType.SpeedUp;

            if (isSbSession) 
            {
                map.AddPoint(ItemType.Invulnerability, new Vector2Int(-xBound + PowerUpPadding, yBound - PowerUpPadding));
                map.AddPoint(ItemType.Invulnerability, new Vector2Int(xBound - PowerUpPadding, -yBound + PowerUpPadding));
                map.AddPoint(ItemType.Invulnerability, new Vector2Int(0, yBound - PowerUpPadding));
                map.AddPoint(ItemType.Invulnerability, new Vector2Int(0, -yBound + PowerUpPadding));
            }

            map.AddPoint(speedModifierType, new Vector2Int(xBound - PowerUpPadding, yBound - PowerUpPadding));
            map.AddPoint(speedModifierType, new Vector2Int(-xBound + PowerUpPadding, -yBound + PowerUpPadding));
            map.AddPoint(speedModifierType, new Vector2Int(xBound - PowerUpPadding, 0));
            map.AddPoint(speedModifierType, new Vector2Int(-xBound + PowerUpPadding, 0));
            
            if (isSbSession) map.ShuffleAll(_rngService);
            else map.ShuffleAll();

            return map;
        }
    }
}