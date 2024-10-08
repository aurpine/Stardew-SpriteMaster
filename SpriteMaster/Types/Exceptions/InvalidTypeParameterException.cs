﻿using SpriteMaster.Extensions;
using System;

namespace SpriteMaster.Types.Exceptions;

/// <summary>
/// Indicates that the generic type parameter is invalid or does not match the required constraints.
/// </summary>
internal class InvalidTypeParameterException : InvalidOperationException {
    internal readonly Type? Type = null;

    /// <summary>Initializes a new instance of the <see cref="InvalidTypeParameterException" /> <see langword="class"/>.</summary>
    internal InvalidTypeParameterException()
        : this("Type Parameter is invalid") {
    }

    /// <summary>Initializes a new instance of the <see cref="InvalidTypeParameterException" /> <see langword="class"/>.</summary>
    /// <param name="type">The invalid <seealso cref="Type"/>.</param>
    internal InvalidTypeParameterException(Type type)
        : this($"Type Parameter '{type.GetTypeName()}' is invalid") {
        Type = type;
    }

    /// <summary>Initializes a new instance of the <see cref="InvalidTypeParameterException" /> <see langword="class"/> with a specified <paramref name="message">error message</paramref>.</summary>
    /// <param name="message">The message that describes the error.</param>
    internal InvalidTypeParameterException(string message)
        : base(message) {
        HResult = -2146233079;
    }

    /// <summary>Initializes a new instance of the <see cref="InvalidTypeParameterException" /> <see langword="class"/> with a specified <paramref name="message">error message</paramref>.</summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="type">The invalid <seealso cref="Type"/>.</param>
    internal InvalidTypeParameterException(string message, Type type)
        : base(message) {
        HResult = -2146233079;
        Type = type;
    }

    /// <summary>Initializes a new instance of the <see cref="InvalidTypeParameterException" /> <see langword="class"/> with a specified <paramref name="message">error message</paramref> and a reference to the <paramref name="innerException">inner exception</paramref> that is the cause of this <see cref="Exception" />.</summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    internal InvalidTypeParameterException(string message, Exception innerException)
        : base(message, innerException) {
        HResult = -2146233079;
    }

    /// <summary>Initializes a new instance of the <see cref="InvalidTypeParameterException" /> <see langword="class"/> with a specified <paramref name="message">error message</paramref> and a reference to the <paramref name="innerException">inner exception</paramref> that is the cause of this <see cref="Exception" />.</summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="type">The invalid <seealso cref="Type"/>.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    internal InvalidTypeParameterException(string message, Type type, Exception innerException)
        : base(message, innerException) {
        HResult = -2146233079;
        Type = type;
    }
}

/// <summary>
/// Indicates that the generic type parameter (<seealso cref="TParameter"/>) is invalid or does not match the required constraints.
/// </summary>
internal class InvalidTypeParameterException<TParameter> : InvalidTypeParameterException {
    private static readonly string DefaultMessage = string.Intern($"Type Parameter '{typeof(TParameter).GetTypeName()}' is invalid");

    /// <summary>Initializes a new instance of the <see cref="InvalidTypeParameterException{TParameter}" /> <see langword="class"/>.</summary>
    internal InvalidTypeParameterException()
        : this(DefaultMessage) {
    }

    /// <summary>Initializes a new instance of the <see cref="InvalidTypeParameterException{TParameter}" /> <see langword="class"/> with a specified <paramref name="message">error message</paramref>.</summary>
    /// <param name="message">The message that describes the error.</param>
    internal InvalidTypeParameterException(string message)
        : base(message ?? DefaultMessage, typeof(TParameter)) {
        this.HResult = -2146233079;
    }

    /// <summary>Initializes a new instance of the <see cref="InvalidTypeParameterException{TParameter}" /> <see langword="class"/> with a specified <paramref name="message">error message</paramref> and a reference to the <paramref name="innerException">inner exception</paramref> that is the cause of this <see cref="Exception" />.</summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    internal InvalidTypeParameterException(string message, Exception innerException)
        : base(message ?? DefaultMessage, typeof(TParameter), innerException) {
        this.HResult = -2146233079;
    }
}
