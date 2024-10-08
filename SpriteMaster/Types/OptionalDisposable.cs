﻿using System;
using System.Collections.Generic;

namespace SpriteMaster.Types;

internal ref struct OptionalDisposable<T> where T : IDisposable {
    internal T? Value = default;

    internal readonly bool HasValue =>
        !EqualityComparer<T?>.Default.Equals(Value, default);

    public OptionalDisposable() {
    }

    public OptionalDisposable(T? value) {
        Value = value;
    }

    public void Dispose() {
        Value?.Dispose();
        Value = default;
    }

    public static implicit operator OptionalDisposable<T>(T? value) => new(value);
    public static implicit operator T?(OptionalDisposable<T> value) => value.Value;
}
