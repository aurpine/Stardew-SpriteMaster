﻿using System;
using System.Collections.Generic;

namespace SpriteMaster.Types.Exceptions;

internal class UnknownArgumentEnumException<TArgument> : UnknownArgumentException<TArgument>, IUnknownEnumException<TArgument>
    where TArgument : unmanaged, Enum {
    private static string GetDefaultMessage(in TArgument value) => $"Unknown Argument Enum Value: '{value}'";

    TArgument IUnknownEnumException<TArgument>.Value => Value;

    IEnumerable<TArgument> IUnknownEnumException<TArgument>.LegalValues => IUnknownEnumException<TArgument>.StaticLegalValues.Value;

    internal UnknownArgumentEnumException(string paramName, in TArgument value) : this(paramName, value, GetDefaultMessage(value)) {
    }

    internal UnknownArgumentEnumException(string paramName, in TArgument value, string message) : base(paramName, value, message) {
    }
}
