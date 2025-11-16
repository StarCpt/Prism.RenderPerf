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
    }
}
