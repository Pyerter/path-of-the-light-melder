using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteShaderController : MonoBehaviour
{
    public const string BASE_MAT_NAME = "_base_material";

    [SerializeField] protected SpriteRenderer spriteRenderer;
    public SpriteRenderer SpriteRenderer { get { return spriteRenderer; } }

    [SerializeField] protected List<Material> materials = new List<Material>();
    public IReadOnlyList<Material> Materials { get {  return materials.AsReadOnly(); } }

    protected bool materialsListUpdated = false;
    public bool MaterialsListUpdated { get { return materialsListUpdated; } }

    protected NamedMaterial.NamedMaterialIndexComparer namedMaterialComparer = new NamedMaterial.NamedMaterialIndexComparer();

    protected int nextIndex = 0;

    [SerializeField] protected Material baseMaterial;
    [SerializeField] protected Dictionary<string, NamedMaterial> mappedMaterials = new Dictionary<string, NamedMaterial>();
    public Dictionary<string, NamedMaterial>.ValueCollection MappedMaterials { get { return mappedMaterials.Values; } }

    public Material this[int index]
    {
        get
        {
            if (index >= materials.Count)
                return null;
            return materials[index];
        }
    }

    public Material this[string matName]
    {
        get
        {
            if (mappedMaterials.TryGetValue(matName, out NamedMaterial material))
                return material.Material;
            return null;
        }
        set
        {
            if (mappedMaterials.ContainsKey(matName))
            {
                SwapMaterial(matName, value);
            }
            else
                AddMaterial(matName, value);
        }
    }

    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (baseMaterial == null)
            baseMaterial = spriteRenderer.material;

        mappedMaterials = new Dictionary<string, NamedMaterial>();

        if (baseMaterial != null)
        {
            TryAddMaterial(BASE_MAT_NAME, baseMaterial);
        }
    }

    public void UpdateMaterialsList()
    {
        List<NamedMaterial> mats = new List<NamedMaterial>();
        foreach (NamedMaterial mat in mappedMaterials.Values)
        {
            GridUtility.InsertIntoSortedList(mats, mat, namedMaterialComparer);
        }

        materials = mats.ConvertAll(mat => mat.Material);
        spriteRenderer.materials = materials.ToArray();
        Debug.Log("Updated sprite shader controller materials, count: " + materials.Count);
        materialsListUpdated = true;
    }

    public void AddMaterial(string matName, Material mat)
    {
        NamedMaterial newMat = NamedMaterial.CreateNamedMaterial(matName, mat, nextIndex);
        Debug.Log("Named Material: " + (newMat == null ? "null" : newMat.MaterialName));
        mappedMaterials.Add(matName, newMat);
        nextIndex++;
        materialsListUpdated = false;
    }

    public bool TryAddMaterial(string matName, Material material)
    {
        if (mappedMaterials.ContainsKey(matName))
            return false;

        AddMaterial(matName, material);
        return true;
    }

    public bool RemoveMaterial(string matName)
    {
        if (mappedMaterials.TryGetValue(matName, out NamedMaterial mat))
        {
            int matIndex = mat.Index;
            mappedMaterials.Remove(matName);
            if (matIndex != -1)
            {
                foreach (NamedMaterial higherMat in mappedMaterials.Values)
                {
                    if (higherMat.Index > matIndex)
                        higherMat.Index--;
                }
            }
            nextIndex--;
            materialsListUpdated = false;
            return true;
        }
        return false;
    }

    public void SwapMaterial(string  matName, Material mat)
    {
        if (mappedMaterials.TryGetValue(matName, out NamedMaterial namedMat))
        {
            namedMat.Material = mat;
            materialsListUpdated = false;
        }
    }

    private void Update()
    {
        if (!materialsListUpdated)
            UpdateMaterialsList();
    }
}
