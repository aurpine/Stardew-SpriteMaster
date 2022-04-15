﻿#if !SHIPPING
using System;

namespace SpriteMaster.Resample.Scalers.SuperXBR.Cg;

static class CgMath {
	internal static Float3 Min4(in Float3 a, in Float3 b, in Float3 c, in Float3 d) {
		var m0 = a.Min(b);
		var m1 = c.Min(d);
		return m0.Min(m1);
	}

	internal static Float4 Min4(in Float4 a, in Float4 b, in Float4 c, in Float4 d) {
		var m0 = a.Min(b);
		var m1 = c.Min(d);
		return m0.Min(m1);
	}

	internal static Float3 Max4(in Float3 a, in Float3 b, in Float3 c, in Float3 d) {
		var m0 = a.Max(b);
		var m1 = c.Max(d);
		return m0.Max(m1);
	}

	internal static Float4 Max4(in Float4 a, in Float4 b, in Float4 c, in Float4 d) {
		var m0 = a.Max(b);
		var m1 = c.Max(d);
		return m0.Max(m1);
	}

	// https://thebookofshaders.com/glossary/?search=smoothstep
	internal static float SmoothStep(float edge0, float edge1, float x) {
		float t = Math.Clamp((x - edge0) / (edge1 - edge0), 0.0f, 1.0f);
		return t * t * (3.0f - 2.0f * t);
	}

	internal static float Lerp(float x, float y, float s) => x + s * (y - x);

	internal static Float3 Lerp(in Float3 x, in Float3 y, in Float3 s) => (
		Lerp(x.X, y.X, s.X),
		Lerp(x.Y, y.Y, s.Y),
		Lerp(x.Z, y.Z, s.Z)
	);

	internal static Float4 Lerp(in Float4 x, in Float4 y, in Float4 s) => (
		Lerp(x.X, y.X, s.X),
		Lerp(x.Y, y.Y, s.Y),
		Lerp(x.Z, y.Z, s.Z),
		Lerp(x.W, y.W, s.W)
	);

	internal static Float3 Lerp(in Float3 x, in Float3 y, float s) => (
		Lerp(x.X, y.X, s),
		Lerp(x.Y, y.Y, s),
		Lerp(x.Z, y.Z, s)
	);

	internal static Float4 Lerp(in Float4 x, in Float4 y, float s) => (
		Lerp(x.X, y.X, s),
		Lerp(x.Y, y.Y, s),
		Lerp(x.Z, y.Z, s),
		Lerp(x.W, y.W, s)
	);

	internal static float Step(float y, float x) {
		return (x >= y) ? 1.0f : 0.0f;
	}

	internal static Float3 Step(in Float3 y, in Float3 x) => (
		Step(y.X, x.X),
		Step(y.Y, x.Y),
		Step(y.Z, x.Z)
	);

	internal static Float3 MatrixMul(in this Float3 vec, in Float3 m0, in Float3 m1, in Float3 m2) => (
		vec.X * m0.X + vec.Y * m1.X + vec.Z * m2.X,
		vec.X * m0.Y + vec.Y * m1.Y + vec.Z * m2.Y,
		vec.X * m0.Z + vec.Y * m1.Z + vec.Z * m2.Z
	);

	internal static Float3 MatrixMul(in this Float4 vec, in Float3 m0, in Float3 m1, in Float3 m2, in Float3 m3) => (
		vec.X * m0.X + vec.Y * m1.X + vec.Z * m2.X + vec.W * m3.X,
		vec.X * m0.Y + vec.Y * m1.Y + vec.Z * m2.Y + vec.W * m3.Y,
		vec.X * m0.Z + vec.Y * m1.Z + vec.Z * m2.Z + vec.W * m3.Z
	);

	internal static Float4 MatrixMul(in this Float4 vec, in Float4 m0, in Float4 m1, in Float4 m2, in Float4 m3) => (
		vec.X * m0.X + vec.Y * m1.X + vec.Z * m2.X + vec.W * m3.X,
		vec.X * m0.Y + vec.Y * m1.Y + vec.Z * m2.Y + vec.W * m3.Y,
		vec.X * m0.Z + vec.Y * m1.Z + vec.Z * m2.Z + vec.W * m3.Z,
		vec.X * m0.W + vec.Y * m1.W + vec.Z * m2.W + vec.W * m3.W
	);

	internal static float Frac(this float f) => f % 1.0f;

	internal static Float2 Frac(in this Float2 vec) => (
		Frac(vec.X),
		Frac(vec.Y)
	);

	private static readonly Float3 Y = (0.2126f, 0.7152f, 0.0722f);

	internal static float ToYUV(in this Float3 color) => color.Dot(Y);

	internal static float ToYUV(in this Float4 color) => color.RGB.Dot(Y);

	internal static DiffTexel ToDiffTexel(in this Float3 color) => new(
		color.ToYUV(),
		1.0f
	);

	internal static DiffTexel ToDiffTexel(in this Float4 color) => new(
		color.ToYUV(),
		color.A
	);

	internal static float Difference(float a, float b) => MathF.Abs(a - b);

	internal static float Difference(in DiffTexel a, in DiffTexel b) {
		float yufDiff = Difference(a.YUV, b.YUV);
		float alphaScalar = MathF.Min(a.Alpha, b.Alpha);
		float alphaDiff = Difference(a.Alpha, b.Alpha);
		return alphaScalar * yufDiff + alphaDiff;
	}
}
#endif
