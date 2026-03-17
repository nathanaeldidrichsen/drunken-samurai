#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class ForgeContentGenerator : EditorWindow
{
    // -----------------------
    // DATA
    // -----------------------

    static string[] ORES =
    {
        "Ember Ore",
        "Frost Ore",
        "Void Ore",
        "Storm Ore",
        "Solar Ore"
    };

    static string[] FRAGMENTS =
    {
        "Fire Core",
        "Phoenix Ash",
        "Ice Shard",
        "Frost Essence",
        "Void Dust",
        "Shadow Fragment",
        "Lightning Spark",
        "Storm Crystal",
        "Sun Fragment",
        "Radiant Core",

        "Blood Shard",
        "Earth Core",
        "Wind Essence",
        "Moon Fragment",
        "Star Dust",

        "Chaos Shard",
        "Aether Piece",
        "Spirit Thread",
        "Time Fragment",
        "Dream Crystal",

        "Corruption Scale",
        "Light Shard",
        "Abyss Piece",
        "Plasma Core",
        "Gravity Chip",

        "Mythic Ember",
        "Arcane Splinter",
        "Astral Dust",
        "Soul Thread",
        "Eclipse Fragment"
    };

    static string[] SWORDS =
    {
        "Flamebrand",
        "Inferno Blade",
        "Frostbite",
        "Glacier Edge",
        "Void Reaver",
        "Shadow Cleaver",
        "Stormbringer",
        "Thunder Fang",
        "Solaris",
        "Sunfall"
    };

    // -----------------------

    const string SwordPath = "Assets/Data/Items/Swords";
    const string OrePath = "Assets/Data/Items/Materials/Ore";
    const string FragPath = "Assets/Data/Items/Materials/Fragments";
    const string RecipePath = "Assets/Data/Recipes";

    // -----------------------

    [MenuItem("Tools/Forge/Generate All Items")]
    public static void ShowWindow()
    {
        GetWindow<ForgeContentGenerator>("Forge Generator");
    }

    void OnGUI()
    {
        GUILayout.Space(10);

        GUILayout.Label("Forge Content Generator", EditorStyles.boldLabel);

        GUILayout.Space(10);

        if (GUILayout.Button("Generate All Items & Recipes", GUILayout.Height(40)))
        {
            if (EditorUtility.DisplayDialog(
                "Generate Content",
                "This will create assets.\nDuplicates may occur.\nContinue?",
                "Yes",
                "Cancel"))
            {
                GenerateAll();
            }
        }
    }

    // -----------------------

    static void GenerateAll()
    {
        CreateFolders();

        var ores = CreateOres();
        var frags = CreateFragments();
        var swords = CreateSwords();

        CreateRecipes(ores, frags, swords);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Forge content generated!");
    }

    // -----------------------

    static void CreateFolders()
    {
        CreateFolder("Assets/Data");
        CreateFolder("Assets/Data/Items");
        CreateFolder("Assets/Data/Items/Materials");
        CreateFolder("Assets/Data/Items/Materials/Ore");
        CreateFolder("Assets/Data/Items/Materials/Fragments");
        CreateFolder("Assets/Data/Items/Swords");
        CreateFolder("Assets/Data/Recipes");
    }

    static void CreateFolder(string path)
    {
        if (!AssetDatabase.IsValidFolder(path))
        {
            string parent = Path.GetDirectoryName(path);
            string name = Path.GetFileName(path);

            AssetDatabase.CreateFolder(parent, name);
        }
    }

    // -----------------------
    // MATERIALS
    // -----------------------

    static Dictionary<string, MaterialItem> CreateOres()
    {
        var dict = new Dictionary<string, MaterialItem>();

        foreach (var name in ORES)
        {
            MaterialItem item = ScriptableObject.CreateInstance<MaterialItem>();

            item.itemName = name;
            item.type = ItemType.Material;
            item.isStackable = true;
            item.maxStack = 99;

            item.materialType = MaterialItem.MaterialType.Ore;

            string path = $"{OrePath}/{name}.asset";

            AssetDatabase.CreateAsset(item, path);

            dict.Add(name, item);
        }

        return dict;
    }

    static Dictionary<string, MaterialItem> CreateFragments()
    {
        var dict = new Dictionary<string, MaterialItem>();

        foreach (var name in FRAGMENTS)
        {
            MaterialItem item = ScriptableObject.CreateInstance<MaterialItem>();

            item.itemName = name;
            item.type = ItemType.Material;
            item.isStackable = true;
            item.maxStack = 99;

            item.materialType = MaterialItem.MaterialType.Fragment;

            string path = $"{FragPath}/{name}.asset";

            AssetDatabase.CreateAsset(item, path);

            dict.Add(name, item);
        }

        return dict;
    }

    // -----------------------
    // SWORDS
    // -----------------------

    static Dictionary<string, EquipmentItem> CreateSwords()
    {
        var dict = new Dictionary<string, EquipmentItem>();

        int baseDamage = 10;

        foreach (var name in SWORDS)
        {
            EquipmentItem sword = ScriptableObject.CreateInstance<EquipmentItem>();

            sword.itemName = name;
            sword.type = ItemType.Equipment;
            sword.isEquipable = true;

            sword.damageStat = baseDamage;
            sword.defenseStat = baseDamage / 3;
            sword.healthStat = baseDamage * 2;
            sword.moveSpeedStat = 0.1f;

            baseDamage += 5;

            string path = $"{SwordPath}/{name}.asset";

            AssetDatabase.CreateAsset(sword, path);

            dict.Add(name, sword);
        }

        return dict;
    }

    // -----------------------
    // RECIPES
    // -----------------------

    static void CreateRecipes(
        Dictionary<string, MaterialItem> ores,
        Dictionary<string, MaterialItem> frags,
        Dictionary<string, EquipmentItem> swords)
    {
        int recipeIndex = 0;

        foreach (var sword in swords.Values)
        {
            ForgeRecipe recipe = ScriptableObject.CreateInstance<ForgeRecipe>();

            recipe.ore = GetRandom(ores);
            recipe.fragment1 = GetRandom(frags);
            recipe.fragment2 = GetRandom(frags);

            recipe.results = new List<ForgeResult>();

            // Main result
            recipe.results.Add(new ForgeResult
            {
                resultItem = sword,
                chance = 70f
            });

            // Bonus result
            EquipmentItem bonus = GetRandom(swords);

            if (bonus != sword)
            {
                recipe.results.Add(new ForgeResult
                {
                    resultItem = bonus,
                    chance = 30f
                });
            }

            string path =
                $"{RecipePath}/Recipe_{recipeIndex}_{sword.itemName}.asset";

            AssetDatabase.CreateAsset(recipe, path);

            recipeIndex++;
        }
    }

    // -----------------------

    static T GetRandom<T>(Dictionary<string, T> dict)
    {
        var list = new List<T>(dict.Values);

        return list[Random.Range(0, list.Count)];
    }
}

#endif
