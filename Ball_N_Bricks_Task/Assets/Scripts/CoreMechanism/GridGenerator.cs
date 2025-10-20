using UnityEngine;
using System;
using System.Collections.Generic;

namespace LevelGeneration
{
    public class GridGenerator : MonoBehaviour
    {
        public List<BrickDefinition> brickDefinitions;
        public Vector2 cellSize = new Vector2(3f, 1.5f);
        public float verticalBrickLayoutPosition = 0f;
        public List<GameObject> bricksInstantiated;
        public void PrepareLayout(LevelDefinition levelDefinition)
        {
            var grid = TextureToLevelSampler.CreateGridFromTexture(levelDefinition.levelTexture);
            var cols = grid.GetLength(0);
            var rows = grid.GetLength(1);

            // center whole grid on X = 0: compute starting X for first cell center
            var totalWidth = cols * cellSize.x;
            var xStart = -0.5f * totalWidth + 0.5f * cellSize.x;

            for (var x = 0; x < cols; x++)
            {
                for (var y = 0; y < rows; y++)
                {
                    var cell = grid[x, y];
                    print(cell.ToString());
                    for (int i = 0; i < brickDefinitions.Count; i++)
                    {
                        if (cell == brickDefinitions[i].samplingIdentifier)
                        {
                            var worldX = xStart + x * cellSize.x;
                            var worldY = y * cellSize.y + 0.5f * cellSize.y + verticalBrickLayoutPosition;
                            var newBrick = Instantiate(brickDefinitions[i].brickPrefab, new Vector3(worldX, worldY, 0f),
                                Quaternion.identity);
                            newBrick.Init(brickDefinitions[i]);
                            bricksInstantiated.Add(newBrick.gameObject);
                        }
                    }
                }
            }
        }

        public void CleanUpCurrentLevelRemnants()
        {
            foreach (var brick in bricksInstantiated)
            {
                Destroy(brick);
            }
        }
    }

    [Serializable]
    public class BrickDefinition
    {
        public Brick brickPrefab;
        public Color samplingIdentifier;
        public int hitsToBreak;
    }
}