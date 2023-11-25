using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StringClaimManager
{
    [SerializeField] protected Dictionary<string, StringClaim> claims;

    public StringClaimManager()
    {
        claims = new Dictionary<string, StringClaim>();
    }

    public bool TryClaim(string claim, StringClaim claimer)
    {
        if (claims.ContainsKey(claim))
            return false;
        claims.Add(claim, claimer);
        return true;
    }

    public bool HasClaim(string claim)
    {
        return claims.ContainsKey(claim);
    }

    public StringClaim GetClaim(string claim)
    {
        if (claims.TryGetValue(claim, out StringClaim claimer))
            return claimer;
        return null;
    }

    public bool TryGetClaim(string claim, out StringClaim claimer)
    {
        claimer = GetClaim(claim);
        return claimer != null;
    }
}
