﻿using System;
using System.Runtime.CompilerServices;

namespace SpriteMaster.Extensions;

internal static class LinqExt {
    internal interface IPredicate<T> {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        bool Invoke(T obj);
    }

    #region string

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool AnyF(this string str) => str.Length != 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool AnyF(this string str, Func<char, bool> predicate) {
        foreach (char c in str) {
            if (predicate(c)) {
                return true;
            }
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static unsafe bool AnyF(this string str, delegate* managed<char, bool> predicate) {
        foreach (char c in str) {
            if (predicate(c)) {
                return true;
            }
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static unsafe bool AnyF(this string str, delegate* unmanaged<char, bool> predicate) {
        foreach (char c in str) {
            if (predicate(c)) {
                return true;
            }
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool AnyF<TPredicate>(this string str, TPredicate predicate) where TPredicate : IPredicate<char> {
        foreach (char c in str) {
            if (predicate.Invoke(c)) {
                return true;
            }
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool AllF(this string str, Func<char, bool> predicate) {
        foreach (char c in str) {
            if (!predicate(c)) {
                return false;
            }
        }

        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static unsafe bool AllF(this string str, delegate* managed<char, bool> predicate) {
        foreach (char c in str) {
            if (!predicate(c)) {
                return false;
            }
        }

        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static unsafe bool AllF(this string str, delegate* unmanaged<char, bool> predicate) {
        foreach (char c in str) {
            if (!predicate(c)) {
                return false;
            }
        }

        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool AllF<TPredicate>(this string str, TPredicate predicate) where TPredicate : IPredicate<char> {
        foreach (char c in str) {
            if (!predicate.Invoke(c)) {
                return false;
            }
        }

        return true;
    }

    #endregion
}
