using HarmonyLib;
using System.Reflection;
using System.Reflection.Emit;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace System.Collections.Generic
#pragma warning restore IDE0130 // Namespace does not match folder structure
{
    public static class ListExtensions
    {
        // https://stackoverflow.com/a/17308019
        private static class ArrayAccessor<T>
        {
            public static readonly Func<List<T>, T[]> Getter;

            static ArrayAccessor()
            {
                var dm = new DynamicMethod("get", MethodAttributes.Static | MethodAttributes.Public, CallingConventions.Standard, typeof(T[]), new Type[] { typeof(List<T>) }, typeof(ArrayAccessor<T>), true);
                var il = dm.GetILGenerator();
                il.Emit(OpCodes.Ldarg_0); // Load List<T> argument
                il.Emit(OpCodes.Ldfld, typeof(List<T>).GetField("_items", BindingFlags.NonPublic | BindingFlags.Instance)); // Replace argument by field
                il.Emit(OpCodes.Ret); // Return field
                Getter = (Func<List<T>, T[]>)dm.CreateDelegate(typeof(Func<List<T>, T[]>));
            }
        }

        public static Span<T> AsSpan<T>(this List<T> list)
        {
            return ArrayAccessor<T>.Getter.Invoke(list).AsSpan(0, list.Count);
        }

        public static Span<T> AsSpan<T>(this List<T> list, int startIndex)
        {
            return ArrayAccessor<T>.Getter.Invoke(list).AsSpan(startIndex, list.Count - startIndex);
        }

        public static Span<T> AsSpan<T>(this List<T> list, int startIndex, int length)
        {
            if (startIndex + length > list.Count)
                length = list.Count - startIndex;
            return ArrayAccessor<T>.Getter.Invoke(list).AsSpan(startIndex, length);
        }

        private static class ListAccessor<T>
        {
            public static readonly AccessTools.FieldRef<List<T>, int> SizeFieldRef;
            public static readonly AccessTools.FieldRef<List<T>, int> VersionFieldRef;

            static ListAccessor()
            {
                SizeFieldRef    = AccessTools.FieldRefAccess<List<T>, int>(typeof(List<T>).GetField("_size", BindingFlags.NonPublic | BindingFlags.Instance));
                VersionFieldRef = AccessTools.FieldRefAccess<List<T>, int>(typeof(List<T>).GetField("_version", BindingFlags.NonPublic | BindingFlags.Instance));
            }
        }

        /// <summary>
        /// Clear the list without zeroing the internal array.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        public static void ClearFast<T>(this List<T> list)
        {
            if (list.Count > 0)
            {
                ListAccessor<T>.SizeFieldRef(list) = 0;
                ListAccessor<T>.VersionFieldRef(list)++;
            }
        }

        public static ref T AddRef<T>(this List<T> list)
        {
            int capacity = list.Capacity;
            if (capacity == list.Count)
            {
                int newCapacity = capacity == 0 ? 4 : (capacity * 2);
                if ((uint)newCapacity > 2146435071u)
                {
                    newCapacity = 2146435071;
                }
                list.Capacity = newCapacity;
            }

            ListAccessor<T>.VersionFieldRef(list)++;
            return ref ArrayAccessor<T>.Getter(list)[ListAccessor<T>.SizeFieldRef(list)++];
        }
    }
}
