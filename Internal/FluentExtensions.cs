using System;

namespace SimpleDiscordNet_DemoApp.Internal;

/// <summary>
/// Small fluent helpers used in the demo to keep builder-style code readable
/// without introducing temporary variables for conditional configuration.
/// </summary>
internal static class FluentExtensions
{
    /// <summary>
    /// Conditionally applies a transformation function to <paramref name="source"/> and returns the (possibly) transformed value.
    /// Useful to keep fluent/chainable APIs concise when a step should run only under a condition.
    /// </summary>
    /// <typeparam name="T">Type of the value being transformed.</typeparam>
    /// <param name="source">The initial value.</param>
    /// <param name="condition">When true, <paramref name="whenTrue"/> is applied; otherwise the original value is returned.</param>
    /// <param name="whenTrue">Function to apply when the condition is true.</param>
    /// <returns>The transformed value when the condition is true; otherwise the original value.</returns>
    public static T If<T>(this T source, bool condition, Func<T, T> whenTrue) => condition ? whenTrue(source) : source;
}
