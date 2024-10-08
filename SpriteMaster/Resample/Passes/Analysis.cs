﻿using SpriteMaster.Colors;
using SpriteMaster.Configuration;
using SpriteMaster.Extensions;
using SpriteMaster.Types;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SpriteMaster.Resample.Passes;

internal static class Analysis {
    [StructLayout(LayoutKind.Auto)]
    internal readonly struct LegacyResults {
        internal readonly Vector2B Wrapped;
        internal readonly QuadB Repeat;
        internal readonly QuadB Edge;
        internal readonly Vector2B GradientAxial;
        internal readonly Vector2B GradientDiagonal;
        internal readonly int OpaqueCount;
        internal readonly int MaxChannelShades;

        [MethodImpl(Runtime.MethodImpl.Inline)]
        internal LegacyResults(
            Vector2B wrapped,
            QuadB repeat,
            QuadB edge,
            Vector2B gradientAxial,
            Vector2B gradientDiagonal,
            int opaqueCount,
            int maxChannelShades
        ) {
            Wrapped = wrapped;
            Repeat = repeat;
            Edge = edge;
            GradientAxial = gradientAxial;
            GradientDiagonal = gradientDiagonal;
            OpaqueCount = opaqueCount;
            MaxChannelShades = maxChannelShades;
        }
    }

    internal static unsafe LegacyResults AnalyzeLegacy(ReadOnlySpan<Color8> data, Bounds bounds, Vector2B wrapped, bool strict = true) {
        var boundsInverted = bounds.Invert;

        if (bounds.Width < 0 || bounds.Height < 0) {
            Debug.Error($"Inverted Sprite Bounds Value leaked to AnalyzeLegacy: {bounds}");

            boundsInverted.X = bounds.Width < 0;
            boundsInverted.Y = bounds.Height < 0;

            bounds.Width = Math.Abs(bounds.Width);
            bounds.Height = Math.Abs(bounds.Height);
        }

        float edgeThreshold = Config.WrapDetection.EdgeThreshold;

        if (strict) {
            var ratio = (float)bounds.Extent.MaxOf / (float)bounds.Extent.MinOf;
            edgeThreshold = ratio >= 4.0f ? 2.0f : 0.8f;
        }

        var wrappedXY = wrapped;
        QuadB repeat = QuadB.False;

        if (Config.WrapDetection.Enabled) {
            long numSamples = 0;
            if (!wrappedXY.All) {
                for (int y = 0; y < bounds.Height; ++y) {
                    int offset = (y + bounds.Top) * bounds.Width + bounds.Left;
                    for (int x = 0; x < bounds.Width; ++x) {
                        int address = offset + x;
                        var sample = data[address];
                        ++numSamples;
                    }
                }
            }

            byte alphaThreshold = Config.WrapDetection.AlphaThreshold;

            // Count the fragments that are not alphad out completely on the edges.
            // Both edges must meet the threshold.
            if (!wrappedXY.X) {
                var samples = stackalloc int[] { 0, 0 };
                for (int y = 0; y < bounds.Height; ++y) {
                    int offset = (y + bounds.Top) * bounds.Width + bounds.Left;
                    var sample0 = data[offset];
                    var sample1 = data[offset + (bounds.Width - 1)];

                    if (sample0.A.Value >= alphaThreshold) {
                        samples[0]++;
                    }
                    if (sample1.A.Value >= alphaThreshold) {
                        samples[1]++;
                    }
                }
                int threshold = (bounds.Height * edgeThreshold).NearestInt();
                var aboveThreshold = new Vector2B(samples[0] >= threshold, samples[1] >= threshold);
                if (aboveThreshold.All) {
                    wrappedXY.X = true;
                }
                else {
                    repeat.Horizontal = aboveThreshold;
                }
            }
            if (!wrappedXY.Y) {
                var samples = stackalloc int[] { 0, 0 };
                var offsets = stackalloc int[] { bounds.Top * bounds.Width, (bounds.Bottom - 1) * bounds.Width };
                int sampler = 0;
                for (int i = 0; i < 2; ++i) {
                    var yOffset = offsets[i];
                    for (int x = 0; x < bounds.Width; ++x) {
                        int offset = yOffset + x + bounds.Left;
                        var sample = data[offset];
                        if (sample.A.Value >= alphaThreshold) {
                            samples[sampler]++;
                        }
                    }
                    sampler++;
                }
                int threshold = (bounds.Width * edgeThreshold).NearestInt();
                var aboveThreshold = new Vector2B(samples[0] >= threshold, samples[1] >= threshold);
                if (aboveThreshold.All) {
                    wrappedXY.Y = true;
                }
                else {
                    repeat.Vertical = aboveThreshold;
                }
            }
        }

        if (wrappedXY.Any) {
            // Perform tests against both sides of an edge to see if they match up. If they do not, convert
            // a wrapped edge into a repeat edge
            if (wrappedXY.X) {

            }
            if (wrappedXY.Y) {

            }
        }

        // Gradient analysis
        var gradientAxial = Vector2B.True;
        var gradientDiagonal = Vector2B.False;
        // Horizontal
        {
            for (int y = bounds.Top; gradientAxial.X && y < bounds.Bottom; ++y) {
                int offset = (y * bounds.Width) + bounds.Left;
                var prevColor = data[offset];
                for (int x = 1; x < bounds.Width; ++x) {
                    var currColor = data[offset + x];

                    var difference = prevColor.RedmeanDifference(currColor, linear: false, alpha: true);

                    if (difference >= Config.Resample.Analysis.MaxGradientColorDifference) {
                        gradientAxial.X = false;
                        break;
                    }

                    prevColor = currColor;
                }
            }
        }
        // Vertical
        {
            int offset = (bounds.Top * bounds.Width) + bounds.Left;
            var prevColor = data[offset];
            for (int y = 1; gradientAxial.Y && y < bounds.Height; ++y) {
                for (int x = 0; x < bounds.Width; ++x) {
                    var currColor = data[offset + (y * bounds.Width) + x];
                    var difference = prevColor.RedmeanDifference(currColor, linear: false, alpha: true);

                    if (difference >= Config.Resample.Analysis.MaxGradientColorDifference) {
                        gradientAxial.Y = false;
                        break;
                    }

                    prevColor = currColor;
                }
            }
        }
        // Diagonal
        // TODO : use Bresenham's Line Algorithm (https://en.wikipedia.org/wiki/Bresenham%27s_line_algorithm)

        Span<int> shadesR = stackalloc int[byte.MaxValue + 1];
        shadesR.Fill(0);
        Span<int> shadesG = stackalloc int[byte.MaxValue + 1];
        shadesG.Fill(0);
        Span<int> shadesB = stackalloc int[byte.MaxValue + 1];
        shadesB.Fill(0);
        Span<int> shadesA = stackalloc int[byte.MaxValue + 1];
        shadesA.Fill(0);
        foreach (var color in data) {
            ++shadesR[color.R.Value];
            ++shadesG[color.G.Value];
            ++shadesB[color.B.Value];
            ++shadesA[color.A.Value];
        }

        static int NumShades(Span<int> shades) {
            int total = 0;
            foreach (var count in shades) {
                if (count > 0) {
                    total++;
                }
            }
            return total;
        }

        int maxNumShades = Math.Max(
            Math.Max(
                NumShades(shadesR),
                NumShades(shadesG)
            ),
            Math.Max(
                NumShades(shadesB),
                NumShades(shadesA)
            )
        );

        // MinimumGradientShades

        // TODO : Should we flip these values based upon boundsInverted?
        return new(
            wrapped: wrappedXY,
            repeat: repeat,
            edge: QuadB.False,
            gradientAxial: gradientAxial,
            gradientDiagonal: gradientDiagonal,
            opaqueCount: shadesA[255],
            maxChannelShades: maxNumShades
        );
    }

    [MethodImpl(Runtime.MethodImpl.Inline)]
    internal static LegacyResults AnalyzeLegacy(XTexture2D? reference, ReadOnlySpan<Color8> data, Bounds bounds, Vector2B wrapped) {
        return AnalyzeLegacy(
            data: data,
            bounds: bounds,
            wrapped: wrapped,
            strict: reference is not null && !reference.Anonymous() && Config.Resample.Padding.StrictList.Contains(reference.NormalizedName())
        );
    }
}
