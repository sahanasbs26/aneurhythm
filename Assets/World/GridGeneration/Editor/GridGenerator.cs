using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.Tilemaps;

/// <summary>
/// Generates a 3D tilemap grid.
/// To use:
///     In the Project menu in Unity, right click -> create -> scriptable object -> GridData
///     Populate the fields of the GridData object
///     Click on the GridData object so its selected, then at the top menu go to Tools -> Generate Grid.
/// </summary>
public class GridGenerator : Editor
{
    [MenuItem("Tools/Generate Grid")]
    public static void GenerateGrid()
    {
        GridData gridData = Selection.activeObject as GridData;
        if (gridData == null) {Debug.LogError("Select GridData object to generate a grid."); return; }

        GameObject temp = new GameObject(gridData.prefabName);

        InstantiateGrid(temp, gridData);

        if (!Directory.Exists("Assets/World/GridPrefabs"))
            AssetDatabase.CreateFolder("Assets/World", "GridPrefabs");
        string localPath = "Assets/World/GridPrefabs/" + temp.name + ".prefab";
        localPath = AssetDatabase.GenerateUniqueAssetPath(localPath);

        bool success = false;
        GameObject createdPrefab = PrefabUtility.SaveAsPrefabAssetAndConnect(temp, localPath, InteractionMode.UserAction, out success);
        if (success)
        {
            Debug.Log("Successfully created grid prefab at " + localPath);
            EditorGUIUtility.PingObject(createdPrefab);
            
        } else
        {
            Debug.LogError("Error creating grid prefab");
        }

        DestroyImmediate(temp);
    }

    /// <summary>
    /// Adds grid components to the provided GameObject based on GridData.
    /// Adds components & adjusts settings directly.
    /// </summary>
    public static void InstantiateGrid(GameObject obj, GridData gridData)
    {
        //Create tilemap grid
        Grid grid = obj.AddComponent<Grid>();
        grid.cellSwizzle = GridLayout.CellSwizzle.XZY;

        //Add tilemap levels
        for (int i = 0; i < gridData.height; i++)
        {
            GameObject level = new GameObject("Level " + i);
            Tilemap tilemap = level.AddComponent<Tilemap>();
            tilemap.orientation = Tilemap.Orientation.XZ;
            level.AddComponent<TilemapRenderer>();
            level.transform.position = Vector3.up * i;

            level.transform.SetParent(obj.transform, false);
        }
    }

}
