using System;
using System.Runtime.CompilerServices;
using System.Security;
// ReSharper disable StringLiteralTypo

// https://stackoverflow.com/questions/24802222/performance-of-expression-trees#comment44537873_24802222
[assembly: CLSCompliant(false)]
[assembly: AllowPartiallyTrustedCallers]
[assembly: SecurityTransparent]
[assembly: SecurityRules(SecurityRuleSet.Level2, SkipVerificationInFullTrust = true)]
[assembly: ChangeList("5e4303c:0.15.0-beta.6-3-g5e4303c")]
[assembly: BuildComputerName("Palatinate")]
[assembly: FullVersion("0.15.0.106.0-beta.6.0")]

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