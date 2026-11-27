using System.Runtime.InteropServices;

namespace Cliperino.Audio;

sealed class StereoDownmix
{
    const float Attenuated = 0.70710677f;
    const float Shared = 0.5f;

    static readonly Dictionary<int, (float Left, float Right)> Speakers = new()
    {
        [0x1] = (1, 0),
        [0x2] = (0, 1),
        [0x4] = (Attenuated, Attenuated),
        [0x8] = (0, 0),
        [0x10] = (Attenuated, 0),
        [0x20] = (0, Attenuated),
        [0x40] = (1, 0),
        [0x80] = (0, 1),
        [0x100] = (Shared, Shared),
        [0x200] = (Attenuated, 0),
        [0x400] = (0, Attenuated),
        [0x800] = (Shared, Shared),
        [0x1000] = (Attenuated, 0),
        [0x2000] = (Shared, Shared),
        [0x4000] = (0, Attenuated),
        [0x8000] = (Attenuated, 0),
        [0x10000] = (Shared, Shared),
        [0x20000] = (0, Attenuated),
    };

    static readonly Dictionary<int, int> DefaultMasks = new()
    {
        [3] = 0xB,
        [4] = 0x107,
        [5] = 0x37,
        [6] = 0x3F,
        [7] = 0x70F,
        [8] = 0x63F,
    };

    readonly float[] left;
    readonly float[] right;
    byte[] output = [];

    StereoDownmix(float[] left, float[] right)
    {
        this.left = left;
        this.right = right;
    }

    public static StereoDownmix? For(int channels)
    {
        if (channels <= 2) return null;
        int mask = DefaultMasks.GetValueOrDefault(channels, (1 << channels) - 1);

        var left = new float[channels];
        var right = new float[channels];
        for (int channel = 0; channel < channels; channel++)
        {
            int speaker = mask & -mask;
            mask &= mask - 1;
            (left[channel], right[channel]) = Speakers.GetValueOrDefault(speaker, (Shared, Shared));
        }
        return new StereoDownmix(left, right);
    }

    public ReadOnlySpan<byte> Apply(ReadOnlySpan<byte> pcm16)
    {
        int channels = left.Length;
        var input = MemoryMarshal.Cast<byte, short>(pcm16);
        int frames = input.Length / channels;
        if (output.Length < frames * 4) output = new byte[frames * 4];
        var target = MemoryMarshal.Cast<byte, short>(output.AsSpan(0, frames * 4));
        for (int frame = 0; frame < frames; frame++)
        {
            var samples = input.Slice(frame * channels, channels);
            float l = 0, r = 0;
            for (int channel = 0; channel < channels; channel++)
            {
                l += samples[channel] * left[channel];
                r += samples[channel] * right[channel];
            }
            target[frame * 2] = (short)Math.Clamp(l, short.MinValue, short.MaxValue);
            target[frame * 2 + 1] = (short)Math.Clamp(r, short.MinValue, short.MaxValue);
        }
        return output.AsSpan(0, frames * 4);
    }
}
