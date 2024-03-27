using System;
using System.Runtime.CompilerServices;

namespace UnityEngineX
{
    public static class EnumExtensions
    {
        public static unsafe bool Contains<TEnum>(this TEnum lhs, TEnum rhs) where TEnum :
            unmanaged, Enum
        {
            switch (sizeof(TEnum))
            {
                case 1:
                    return (*(byte*)(&lhs) & *(byte*)(&rhs)) > 0;
                case 2:
                    return (*(ushort*)(&lhs) & *(ushort*)(&rhs)) > 0;
                case 4:
                    return (*(uint*)(&lhs) & *(uint*)(&rhs)) > 0;
                case 8:
                    return (*(ulong*)(&lhs) & *(ulong*)(&rhs)) > 0;
                default:
                    throw new Exception("Size does not match a known Enum backing type.");
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void Add<TEnum>(ref this TEnum lhs, TEnum rhs) where TEnum : unmanaged, Enum
        {
            fixed (TEnum* lhs1 = &lhs)
            {
                switch (sizeof(TEnum))
                {
                    case 1:
                    {
                        var r = *(byte*)(lhs1) | *(byte*)(&rhs);
                        *lhs1 = *(TEnum*)&r;
                        return;
                    }
                    case 2:
                    {
                        var r = *(ushort*)(lhs1) | *(ushort*)(&rhs);
                        *lhs1 = *(TEnum*)&r;
                        return;
                    }
                    case 4:
                    {
                        var r = *(uint*)(lhs1) | *(uint*)(&rhs);
                        *lhs1 = *(TEnum*)&r;
                        return;
                    }
                    case 8:
                    {
                        var r = *(ulong*)(lhs1) | *(ulong*)(&rhs);
                        *lhs1 = *(TEnum*)&r;
                        return;
                    }
                    default:
                        throw new Exception("Size does not match a known Enum backing type.");
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void Remove<TEnum>(this ref TEnum lhs, TEnum rhs) where TEnum : unmanaged, Enum
        {
            fixed (TEnum* lhs1 = &lhs)
            {
                switch (sizeof(TEnum))
                {
                    case 1:
                    {
                        var r = *(byte*)(lhs1) & ~*(byte*)(&rhs);
                        *lhs1 = *(TEnum*)&r;
                        return;
                    }
                    case 2:
                    {
                        var r = *(ushort*)(lhs1) & ~*(ushort*)(&rhs);
                        *lhs1 = *(TEnum*)&r;
                        return;
                    }
                    case 4:
                    {
                        var r = *(uint*)(lhs1) & ~*(uint*)(&rhs);
                        *lhs1 = *(TEnum*)&r;
                        return;
                    }
                    case 8:
                    {
                        var r = *(ulong*)(lhs1) & ~*(ulong*)(&rhs);
                        *lhs1 = *(TEnum*)&r;
                        return;
                    }
                    default:
                        throw new Exception("Size does not match a known Enum backing type.");
                }
            }
        }
    }
}