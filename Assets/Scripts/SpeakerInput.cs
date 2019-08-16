using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CSCore;
using CSCore.SoundIn;
using CSCore.DSP;
using CSCore.Streams;
using System;

public class SpeakerInput : MonoBehaviour
{
    private const FftSize CFftSize = FftSize.Fft4096;
    public const int MaxAudioValue = 10;

    public int numBars = 30;
    public int minFreq = 5;
    public int maxFreq = 4500;
    public int barSpacing = 0;
    public bool logScale = true;
    public bool isAverage = false;

    public float highScaleAverage = 2.0f;
    public float highScaleNotAverage = 3.0f;

    float[] fftBuffer;

    WasapiLoopbackCapture loopbackCapture;
    SoundInSource soundInSource;
    SingleBlockNotificationStream singleBlockNotificationStream;
    IWaveSource realTimeSource;

    BasicSpectrumProvider basicSpectrumProvider;
    LineSpectrum lineSpectrum;
    ScalingStrategy scalingStrategy;
    Action<float[]> receiveAudio;

    IWaveSource finalSource;

    // Start is called before the first frame update
    void Start()
    {
        loopbackCapture = new WasapiLoopbackCapture();
        loopbackCapture.Initialize();

        soundInSource = new SoundInSource(loopbackCapture);

        fftBuffer = new float[(int)CFftSize];

        basicSpectrumProvider = new BasicSpectrumProvider(soundInSource.WaveFormat.Channels,
            soundInSource.WaveFormat.SampleRate, CFftSize);

        lineSpectrum = new LineSpectrum(CFftSize)
        {
            SpectrumProvider = basicSpectrumProvider,
            BarCount = numBars,
            UseAverage = true,
            IsXLogScale = false,
            ScalingStrategy = ScalingStrategy.Linear
        };

        var notificationSource = new SingleBlockNotificationStream(soundInSource.ToSampleSource());

        notificationSource.SingleBlockRead += NotificationSource_SingleBlockRead;

        finalSource = notificationSource.ToWaveSource();

        loopbackCapture.DataAvailable += Capture_DataAvailable;
        loopbackCapture.Start();

        //singleBlockNotificationStream = new SingleBlockNotificationStream(soundInSource.ToSampleSource());
        //realTimeSource = singleBlockNotificationStream.ToWaveSource();

        //byte[] buffer = new byte[realTimeSource.WaveFormat.BytesPerSecond / 2];

        //soundInSource.DataAvailable += (s, ea) =>
        //{
        //    while (realTimeSource.Read(buffer, 0, buffer.Length) > 0)
        //    {
        //        float[] spectrumData = lineSpectrum.GetSpectrumData(10);
        //        receiveAudio(spectrumData);
        //        Debug.Log(receiveAudio);

        //        if (spectrumData != null && receiveAudio != null)
        //        {
        //            receiveAudio(spectrumData);
        //            Debug.Log(receiveAudio);
        //        }
        //    }
        //};

        //singleBlockNotificationStream.SingleBlockRead += SingleBlockNotificationStream_SingleBlockRead;
    }

    private void Capture_DataAvailable(object sender, DataAvailableEventArgs e)
    {
        finalSource.Read(e.Data, e.Offset, e.ByteCount);
    }

    private void NotificationSource_SingleBlockRead(object sender, SingleBlockReadEventArgs e)
    {
        basicSpectrumProvider.Add(e.Left, e.Right);
    }

    void OnApplicationQuit()
    {
        if (enabled)
        {
            loopbackCapture.Stop();
            loopbackCapture.Dispose();
        }
    }

    public float[] barData;

    public float[] GetFFtData()
    {
        lock (barData)
        {
            lineSpectrum.BarCount = numBars;
            if (numBars != barData.Length)
            {
                barData = new float[numBars];
            }
        }

        if (basicSpectrumProvider.IsNewDataAvailable)
        {
            lineSpectrum.MinimumFrequency = minFreq;
            lineSpectrum.MaximumFrequency = maxFreq;
            lineSpectrum.IsXLogScale = logScale;
            //lineSpectrum.BarSpacing = barSpacing;
            lineSpectrum.SpectrumProvider.GetFftData(fftBuffer, this);
            return lineSpectrum.GetSpectrumPoints(100.0f, fftBuffer);
        }
        else
        {
            return null;
        }
    }

    // Update is called once per frame
    void Update()
    {
        int numBars = barData.Length;

        float[] resData = GetFFtData();

        if (resData == null)
        {
            return;
        }
        //Debug.Log(barData[1]);
        lock (barData)
        {
            for (int i = 0; i < numBars && i < resData.Length; i++)
            {
                // Make the data between 0.0 and 1.0
                barData[i] = resData[i] / 100.0f;
            }

            for (int i = 0; i < numBars && i < resData.Length; i++)
            {
                if (lineSpectrum.UseAverage)
                {
                    // Scale the data because for some reason bass is always loud and treble is soft
                    barData[i] = barData[i] + highScaleAverage * Mathf.Sqrt(i / (numBars + 0.0f)) * barData[i];
                }
                else
                {
                    barData[i] = barData[i] + highScaleNotAverage * Mathf.Sqrt(i / (numBars + 0.0f)) * barData[i];
                }
            }
        }
    }
}
