using UnityEngine;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using CoreMechanism;
using Misc;

namespace LevelGeneration
{
    public class GridGenerator : MonoBehaviour
    {
        public static GridGenerator Instance;
        public List<BrickDefinition> brickDefinitions;
        public Transform bricksParent;
        public Vector2 cellSize = new Vector2(3f, 1.5f);
        public float verticalBrickLayoutPosition = 0f;
        public List<GameObject> bricksInstantiated;
        public int bricksDestroyedThisLevel;
        public SimpleObjectPooling bricksPool; // TODO Implement more complex <T> object pooling for bricks
        private void Awake() => Instance = this;
        public void PrepareLayout(LevelDefinition levelDefinition)
        {
            // clear previous remnants
            CleanUpCurrentLevelRemnants();
            bricksInstantiated.Clear();
            bricksDestroyedThisLevel = 0;
            var grid = TextureToLevelSampler.CreateGridFromTexture(levelDefinition.levelTexture);
            var cols = grid.GetLength(0);
            var rows = grid.GetLength(1);

            var totalWidth = cols * cellSize.x;
            var xStart = -0.5f * totalWidth + 0.5f * cellSize.x;

            for (var x = 0; x < cols; x++)
            {
                for (var y = 0; y < rows; y++)
                {
                    var cell = grid[x, y];

                    for (int i = 0; i < brickDefinitions.Count; i++)
                    {
                        if (cell == brickDefinitions[i].samplingIdentifier)
                        {
                            var worldX = xStart + x * cellSize.x;
                            var worldY = y * cellSize.y + 0.5f * cellSize.y + verticalBrickLayoutPosition;
                            var newBrick = Instantiate(brickDefinitions[i].brickPrefab, new Vector3(worldX, worldY, 0f),
                                Quaternion.identity, bricksParent);
                            newBrick.Init(brickDefinitions[i]);
                            bricksInstantiated.Add(newBrick.gameObject);
                        }
                    }
                }
            }
            // spawn power-ups by replacing some bricks
            SpawnPowerUps(levelDefinition);
        }

        private void SpawnPowerUps(LevelDefinition levelDefinition)
        {
            if (levelDefinition == null || levelDefinition.powerUps == null || levelDefinition.powerUps.Length == 0)
                return;

            if (bricksInstantiated == null || bricksInstantiated.Count == 0)
                return;

            var availableIndices = new List<int>(bricksInstantiated.Count);
            for (var i = 0; i < bricksInstantiated.Count; i++)
                availableIndices.Add(i);

            foreach (var puConfig in levelDefinition.powerUps)
            {
                if (puConfig == null || puConfig.prefab == null || puConfig.amount <= 0)
                    continue;

                var toSpawn = Mathf.Min(puConfig.amount, availableIndices.Count);

                for (var s = 0; s < toSpawn; s++)
                {
                    var listIndex = UnityEngine.Random.Range(0, availableIndices.Count);
                    var brickIndex = availableIndices[listIndex];
                    availableIndices.RemoveAt(listIndex);

                    var brickGO = bricksInstantiated[brickIndex];
                    if (brickGO == null)
                        continue;

                    var pos = brickGO.transform.position;
                    var rot = brickGO.transform.rotation;
                    Destroy(brickGO);
                    bricksInstantiated[brickIndex] = null; 
                    var puGO = Instantiate(puConfig.prefab, pos, rot);
                    bricksInstantiated[brickIndex] = puGO;
                    bricksDestroyedThisLevel++;
                }
            }

            bricksInstantiated.RemoveAll(item => item == null);
        }

        public void CleanUpCurrentLevelRemnants()
        {
            foreach (var brick in bricksInstantiated)
            {
                if (brick != null)
                    Destroy(brick);
            }

            bricksInstantiated.Clear();
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