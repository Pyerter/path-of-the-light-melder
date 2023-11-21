using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Identity
{
    private static uint nextID;
    private static uint nextIDLoop;
    public static Pair<uint, uint> GetNextID()
    {
        uint result = nextID++;
        if (result >= uint.MaxValue - 1)
        {
            nextID = uint.MaxValue;
            nextIDLoop++;
        }
        return new Pair<uint, uint>(result, nextIDLoop);
    }
    private static int numbMaxString = uint.MaxValue.ToString().Length;
    public static string FormatUIntToMaxLength(uint val)
    {
        return val.ToString("D" + numbMaxString);
    }

    [SerializeField] private Pair<uint, uint> idPair;
    public Pair<uint, uint> IDPair { get { return idPair; } }
    [SerializeField] private string id;
    public string ID { get { return id; } }

    [SerializeField] private string name;
    public string Name { get { return name; } set { if (!NameLock) { name = value; uniqueName = value + ":" + id; NameLock = true; } else { Debug.LogWarning("Unable to change name. Name is locked. Call Identity.UnlockName()."); } } }

    [SerializeField] private bool nameLock = false;
    public bool NameLock { get { return nameLock; } private set { nameLock = value; } }

    [SerializeField] private string uniqueName;
    public string UniqueName { get { return uniqueName; } }

    public Identity(string name = null) : this(GetNextID())
    {
        if (string.IsNullOrEmpty(name))
            this.Name = "Unnamed";
        else
            this.Name = name;

        Debug.Log("ID Generated with name: " + UniqueName);
    }

    private Identity(Pair<uint, uint> idPair)
    {
        this.idPair = idPair;
        id = FormatUIntToMaxLength(idPair.left) + FormatUIntToMaxLength(idPair.right);
    }

    public void UnlockName()
    {
        NameLock = false;
    }
}
