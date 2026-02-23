using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Gameplay.SpawnConfig;
using Utilities;
using Core.Enums;

namespace SpawnPatternsBuilder
{
    [CustomEditor(typeof(SpawnPatternDesigner))]
    public class SpawnPatternDesignerEditor : Editor
    {
        private SpawnPatternDesigner _designer;

        private void OnEnable()
        {
            _designer = (SpawnPatternDesigner)target;
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            GUILayout.Space(10);
            GUILayout.Label("Actions", EditorStyles.boldLabel);

            if (GUILayout.Button("Refresh Visual Preview"))
            {
                _designer.RefreshPreview();
            }

            if (GUILayout.Button("Force Save Assets"))
            {
                AssetDatabase.SaveAssets();
            }

            if (GUILayout.Button("Clear Current Data"))
            {
                if (EditorUtility.DisplayDialog("Clear Data", "Are you sure?", "Yes", "Cancel"))
                {
                    if (_designer.currentMode == SpawnPatternDesigner.EditMode.SpawnPattern)
                        _designer.ClearPattern();
                    else
                        _designer.ClearMap();
                }
            }
            
            GUILayout.Space(10);
        }

        private void OnSceneGUI()
        {
            if (_designer == null) return;

            DrawGrid();
            DrawDataLabels();
            HandleInput();
            
            if (Event.current.type == EventType.MouseMove || Event.current.type == EventType.MouseDrag)
                SceneView.RepaintAll();
        }

        private void DrawGrid()
        {
            if (!_designer.showGrid) return;

            Handles.color = _designer.gridColor;
            float cellSize = _designer.cellSize;
            Vector2Int dims = _designer.boardDimensions;

            float minX = -(dims.x / 2) * cellSize;
            float maxX = (dims.x / 2) * cellSize;
            float minY = -(dims.y / 2) * cellSize;
            float maxY = (dims.y / 2) * cellSize;

            for (int x = 0; x <= dims.x; x++)
            {
                float xPos = minX + (x * cellSize);
                Handles.DrawLine(new Vector3(xPos, minY, 0), new Vector3(xPos, maxY, 0));
            }

            for (int y = 0; y <= dims.y; y++)
            {
                float yPos = minY + (y * cellSize);
                Handles.DrawLine(new Vector3(minX, yPos, 0), new Vector3(maxX, yPos, 0));
            }
        }

        private void DrawDataLabels()
        {
            // We no longer draw cubes here because we have actual prefabs in the scene.
            // We only draw labels to help the designer.
            if (_designer.currentMode == SpawnPatternDesigner.EditMode.SpawnPattern && _designer.targetPattern != null)
            {
                foreach (var item in _designer.targetPattern.items)
                {
                    DrawLabel(item.relativePosition, item.type.ToString());
                }
            }
            else if (_designer.targetMap != null)
            {
                foreach (var group in _designer.targetMap.spawnPoints)
                {
                    foreach (var pos in group.positions)
                    {
                        DrawLabel(pos, group.type.ToString());
                    }
                }
            }
        }

        private void DrawLabel(Vector2Int gridPos, string text)
        {
            Vector3 center = new Vector3(gridPos.x * _designer.cellSize, gridPos.y * _designer.cellSize, 0);
            Handles.Label(center + Vector3.up * (_designer.cellSize * 0.5f), text);
        }

        private void HandleInput()
        {
            Event e = Event.current;

            if (e.type == EventType.MouseDown || e.type == EventType.MouseDrag)
            {
                if (e.button != 0) return;

                Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
                Plane plane = new Plane(Vector3.forward, Vector3.zero);

                if (plane.Raycast(ray, out float enter))
                {
                    Vector3 worldPos = ray.GetPoint(enter);
                    Vector2Int gridPos = WorldToGrid(worldPos);

                    GUIUtility.hotControl = GUIUtility.GetControlID(FocusType.Passive);
                    e.Use();

                    if (e.shift) _designer.RemoveFromPattern(gridPos);
                    else if (_designer.currentMode == SpawnPatternDesigner.EditMode.SpawnPattern) _designer.AddToPattern(gridPos);
                    else _designer.AddToMap(gridPos);
                }
            }
            
            if (e.type == EventType.MouseUp) GUIUtility.hotControl = 0;
        }

        private Vector2Int WorldToGrid(Vector3 worldPos)
        {
            float size = _designer.cellSize;
            int x = Mathf.RoundToInt(worldPos.x / size);
            int y = Mathf.RoundToInt(worldPos.y / size);
            return new Vector2Int(x, y);
        }
    }
}