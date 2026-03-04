using UnityEngine;
using System.Collections.Generic;
using Gameplay.SpawnConfig;
using Core.Enums;
using Gameplay.GameItems;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;

namespace Utilities
{
    [ExecuteInEditMode]
    public class SpawnPatternDesigner : MonoBehaviour
    {
        public enum EditMode
        {
            SpawnPattern,
            SpawnPointMap
        }

        [Header("Configuration")]
        public EditMode currentMode;
        public float cellSize = 0.2f; 
        public Vector2Int boardDimensions = new Vector2Int(22, 22);

        [Header("Target Data")]
        public SpawnPattern targetPattern;
        public SpawnPointMap targetMap;

        [Header("Item Palette")]
        [SerializeField] private List<GameItem> _itemPalette = new();
        [SerializeField] private Transform _previewContainer;

        [Header("Painting Tool")]
        public ItemType brushItem = ItemType.Food; 
        public Color brushColor = Color.green;

        [Header("Visual Settings")]
        public Color gridColor = new Color(1, 1, 1, 0.1f);
        public bool showGrid = true;

        public void RefreshPreview()
        {
            ClearPreviewObjects();

            if (currentMode == EditMode.SpawnPattern && targetPattern != null)
            {
                foreach (var item in targetPattern.items)
                {
                    CreatePreviewObject(item.type, item.relativePosition);
                }
            }
            else if (currentMode == EditMode.SpawnPointMap && targetMap != null)
            {
                foreach (var list in targetMap.spawnPoints)
                {
                    foreach (var pos in list.positions)
                    {
                        CreatePreviewObject(list.type, pos);
                    }
                }
            }
        }

        public void ClearPreviewObjects()
        {
            if (_previewContainer == null) return;

            var children = new List<GameObject>();
            foreach (Transform child in _previewContainer) children.Add(child.gameObject);
            children.ForEach(DestroyImmediate);
        }

        public void ClearPattern()
        {
            if (targetPattern != null)
            {
                targetPattern.items.Clear();
                MarkDirty(targetPattern);
                RefreshPreview();
            }
        }

        public void ClearMap()
        {
            if (targetMap != null)
            {
                targetMap.spawnPoints.Clear();
                MarkDirty(targetMap);
                RefreshPreview();
            }
        }

        private void CreatePreviewObject(ItemType type, Vector2Int gridPos)
        {
            if (_previewContainer == null) return;

            GameItem data = _itemPalette.FirstOrDefault(x => x.type == type);
            if (data == null || data.prefab == null) return;

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(data.prefab, _previewContainer);
            instance.transform.position = new Vector3(gridPos.x * cellSize, gridPos.y * cellSize, 0);
            instance.hideFlags = HideFlags.DontSaveInBuild | HideFlags.DontSaveInEditor;
        }

        public void MarkDirty(Object target)
        {
            EditorUtility.SetDirty(target);
        }

        private void OnDisable()
        {
            ClearPreviewObjects();
        }

        public void AddToPattern(Vector2Int pos)
        {
            if (targetPattern == null) return;
            if (targetPattern.items.Any(x => x.relativePosition == pos)) return;

            targetPattern.items.Add(new PatternItem { type = brushItem, relativePosition = pos });
            MarkDirty(targetPattern);
            RefreshPreview();
        }

        public void RemoveFromPattern(Vector2Int pos)
        {
            if (targetPattern == null) return;
            int removed = targetPattern.items.RemoveAll(x => x.relativePosition == pos);
            if (removed > 0)
            {
                MarkDirty(targetPattern);
                RefreshPreview();
            }
        }

        public void AddToMap(Vector2Int pos)
        {
            if (targetMap == null) return;
            if (targetMap.spawnPoints == null) targetMap.spawnPoints = new List<SpawnPointList>();

            var list = targetMap.spawnPoints.FirstOrDefault(x => x.type == brushItem);
            if (list == null)
            {
                list = new SpawnPointList { type = brushItem, positions = new List<Vector2Int>() };
                targetMap.spawnPoints.Add(list);
            }

            if (!list.positions.Contains(pos))
            {
                list.positions.Add(pos);
                MarkDirty(targetMap);
                RefreshPreview();
            }
        }

        public void RemoveFromMap(Vector2Int pos)
        {
            if (targetMap == null || targetMap.spawnPoints == null) return;

            bool changed = false;
            foreach (var list in targetMap.spawnPoints)
            {
                if (list.positions.Remove(pos)) changed = true;
            }

            if (changed)
            {
                MarkDirty(targetMap);
                RefreshPreview();
            }
        }
    }
}
#endif