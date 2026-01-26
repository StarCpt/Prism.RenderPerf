using System;
using System.Collections.Generic;
using System.Text;

namespace System;

#if !NETCOREAPP
// https://stackoverflow.com/a/1646913
public static class HashCode
{
    private static int Combine(int a, int b)
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 31 + a;
            hash = hash * 31 + b;
            return hash;
        }
    }

    public static int Combine<T>(T a, T b)
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 31 + a.GetHashCode();
            hash = hash * 31 + b.GetHashCode();
            return hash;
        }
    }

    public static int Combine<T>(T a, T b, T c)
    {
        return Combine(Combine(a, b), c.GetHashCode());
    }

    public static int Combine<T>(T a, T b, T c, T d)
    {
        return Combine(Combine(a, b), Combine(c, d));
    }
}
#endif