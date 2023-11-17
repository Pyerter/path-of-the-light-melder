using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IIdentifiable
{
    // To implement, use the following:
    //protected Identity id = new Identity("SomeName");
    //public string ID { get { return id.ID; } }
    //public string UniqueName { get { return id.UniqueName; } }

    public string ID { get; }
    public string UniqueName { get; }
}
