global using XVector2 = Microsoft.Xna.Framework.Vector2;
global using PureAttribute = System.Diagnostics.Contracts.PureAttribute;
using System;
using System.Runtime.CompilerServices;
using System.Security;
// ReSharper disable StringLiteralTypo

// https://stackoverflow.com/questions/24802222/performance-of-expression-trees#comment44537873_24802222
[assembly: CLSCompliant(false)]
[assembly: AllowPartiallyTrustedCallers]
[assembly: SecurityTransparent]
[assembly: InternalsVisibleTo("Preview")]
[assembly: InternalsVisibleTo("Benchmarks.BenchmarkBase")]
[assembly: InternalsVisibleTo("Hashing")]
[assembly: InternalsVisibleTo("Arrays")]
[assembly: InternalsVisibleTo("Sprites")]
[assembly: InternalsVisibleTo("Strings")]
[assembly: InternalsVisibleTo("Math")]
[assembly: SecurityRules(SecurityRuleSet.Level2, SkipVerificationInFullTrust = true)]
[assembly: ChangeList("d271948:0.15.0-beta.10-1-gd271948")]
[assembly: BuildComputerName("Palatinate")]
[assembly: FullVersion("0.15.0.111.0-beta.11.0")]

[module: CLSCompliant(false)]
[module: SkipLocalsInit]

[AttributeUsage(validOn: AttributeTargets.Assembly)]
internal sealed class ChangeListAttribute : Attribute {
	internal readonly string Value;
	internal ChangeListAttribute(string value) => Value = value;
}

[AttributeUsage(validOn: AttributeTargets.Assembly)]
internal sealed class BuildComputerNameAttribute : Attribute {
	internal readonly string Value;
	internal BuildComputerNameAttribute(string value) => Value = value;
}

[AttributeUsage(validOn: AttributeTargets.Assembly)]
internal sealed class FullVersionAttribute : Attribute {
	internal readonly string Value;
	internal FullVersionAttribute(string value) => Value = value;
}