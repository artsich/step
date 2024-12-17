using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using OpenTK.Audio.OpenAL;

namespace Step.Main.Audio;

internal static class NAudioLoader
{
	/// <summary>
	/// Decodes any file NAudio can read into 16-bit PCM, then uploads it to an OpenAL buffer.
	/// </summary>
	public static Sound LoadSound(string filePath)
	{
		// AudioFileReader decodes WAV, MP3, etc. into a floating-point stream (ISampleProvider).
		using var audioFile = new AudioFileReader(filePath);

		// Convert the float samples to 16-bit PCM using SampleToWaveProvider16.
		// This avoids legacy ACM. It's a pure managed conversion.
		var wave16 = new SampleToWaveProvider16(audioFile);

		// Optional: If you need forced stereo or forced sample rate, you could chain a MediaFoundationResampler, etc.
		// For example:
		// var resampler = new MediaFoundationResampler(wave16, new WaveFormat(44100, 16, 2));

		// Write the 16-bit PCM data into a byte[].
		byte[] pcmData;
		using (var ms = new MemoryStream())
		{
			byte[] buffer = new byte[4096];
			int bytesRead;
			while ((bytesRead = wave16.Read(buffer, 0, buffer.Length)) > 0)
			{
				ms.Write(buffer, 0, bytesRead);
			}
			pcmData = ms.ToArray();
		}

		// Now figure out AL format from wave16.WaveFormat.
		var waveFormat = wave16.WaveFormat;
		int channels = waveFormat.Channels;           // Should be 1 (mono) or 2 (stereo)
		int bitsPerSample = waveFormat.BitsPerSample; // Should be 16
		int sampleRate = waveFormat.SampleRate;

		ALFormat alFormat;
		if (channels == 1 && bitsPerSample == 8) alFormat = ALFormat.Mono8;
		else if (channels == 1 && bitsPerSample == 16) alFormat = ALFormat.Mono16;
		else if (channels == 2 && bitsPerSample == 8) alFormat = ALFormat.Stereo8;
		else if (channels == 2 && bitsPerSample == 16) alFormat = ALFormat.Stereo16;
		else
		{
			throw new NotSupportedException(
				$"Not supported audio format: {channels} channels, {bitsPerSample} bits per sample."
			);
		}

		int bufferId = AL.GenBuffer();
		AL.BufferData(bufferId, alFormat, ref pcmData[0], pcmData.Length, sampleRate);

		return new Sound(bufferId);
	}
}
