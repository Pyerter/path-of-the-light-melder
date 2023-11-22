using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NamedMaterial : MonoBehaviour
{
    [SerializeField] protected Material material;
    public Material Material { get { return material; } set { material = value; } }

    [SerializeField] protected string materialName;
    public string MaterialName { get { return materialName; } }

    [SerializeField] protected int index = -1;
    public int Index { get { return index; } set { index = value; } }

    protected void Init(string materialName, Material material, int index = -1)
    {
        this.materialName = materialName;
        this.material = material;
        this.index = index;
    }

    public static NamedMaterial CreateNamedMaterial(string materialName, Material material, int index = -1)
    {
        GameObject go = new GameObject(materialName);
        go.hideFlags = HideFlags.HideInHierarchy;
        NamedMaterial namedMaterial = go.AddComponent<NamedMaterial>();
        namedMaterial.Init(materialName, material, index);
        return namedMaterial;
    }

    public Material SetToIndex(int index)
    {
        this.index = index;
        return Material;
    }

    public class NamedMaterialIndexComparer : IComparer<NamedMaterial>
    {
        int IComparer<NamedMaterial>.Compare(NamedMaterial a, NamedMaterial b)
        {
            return a.index - b.index;
        }
    }
}
