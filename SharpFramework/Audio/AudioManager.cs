using Un4seen.Bass;
using Un4seen.Bass.AddOn.Fx;
using Un4seen.Bass.AddOn.Mix;
using Un4seen.BassWasapi;

namespace SharpFramework.Audio
{
    /// <summary>
    /// オーディオの管理を行うクラス。
    /// </summary>
    public static class AudioManager
    {
        public static List<(int Handle, long Bytes)> ListProcOverlaps = new();
        public static BASS_WASAPI_DEVICEINFO deviceInfo;
        public static long DesiredBufferSize, RBufferSize, Interval, RDelaySize;
        public static int nDevNo;
        public static readonly int[] GainCenters = { 32, 64, 125, 250, 500, 1000, 2000, 4000, 8000, 16000 };
        public static int Mixer;

        private static int _mixer_deviceOut = -1;
        private static WASAPIPROC _wasapiProc = null;
        private static List<Audio> _audios = new List<Audio>();

        public static void Register(Audio audio)
        {
            _audios.Add(audio);
        }

        public static void Update()
        {
            foreach (var audio in _audios)
            {
                if (audio.IsPlaying)
                    audio.CheckAndLoop();
            }
        }

        public static void Clear()
        {
            _audios.Clear();
        }

        /// <summary>
        /// BASS関連のすべてのリソースを解放
        /// </summary>
        public static void Free()
        {
            Bass.FreeMe();
            Bass.BASS_Free();
            BassMix.FreeMe();
            BassFx.FreeMe();
            BassWasapi.FreeMe();
            BassWasapi.BASS_WASAPI_Free();
            for (int index = ListProcOverlaps.Count - 1; index >= 0; index--)
            {
                BassMix.BASS_Mixer_ChannelRemove(ListProcOverlaps[index].Handle);
                Bass.BASS_StreamFree(ListProcOverlaps[index].Handle);
                ListProcOverlaps.RemoveAt(index);
            }
        }

        /// <summary>
        /// WASAPIとBASSミキサーを初期化
        /// </summary>
        public static void Init(bool isWasapiExclusive = false)
        {
            Free();
            int deviceId = 0;
            int hertz = 44100;
            int channel = 0;
            BassNet.Registration("dtx2013@gmail.com", "2X9181017152222");
            Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_UPDATEPERIOD, 0);
            Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_UPDATETHREADS, 0);
            Bass.BASS_Init(deviceId, hertz, BASSInit.BASS_DEVICE_LATENCY, nint.Zero);
            Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_CURVE_VOL, newvalue: true);
            deviceId = -1;
            hertz = 0;
            BASSWASAPIInit flags = isWasapiExclusive ? BASSWASAPIInit.BASS_WASAPI_EXCLUSIVE | BASSWASAPIInit.BASS_WASAPI_BUFFER : BASSWASAPIInit.BASS_WASAPI_BUFFER;
            _wasapiProc = WasapiProc;
            for (int device = 0; (deviceInfo = BassWasapi.BASS_WASAPI_GetDeviceInfo(device)) != null; device++)
            {
                if (deviceInfo.IsDefault)
                {
                    nDevNo = device;
                    break;
                }
            }
            if (nDevNo != -1)
            {
                Interval = (long)(deviceInfo.minperiod * 1000.0);
                if (DesiredBufferSize <= 0 || DesiredBufferSize < Interval + 1)
                {
                    long num4 = Interval + 1;
                    DesiredBufferSize = (int)num4;
                }
            }
            if (BassWasapi.BASS_WASAPI_Init(deviceId, hertz, channel, flags, DesiredBufferSize / 1000f, Interval / 1000f, _wasapiProc, nint.Zero))
            {
                if (isWasapiExclusive)
                {
                    nDevNo = BassWasapi.BASS_WASAPI_GetDevice();
                    deviceInfo = BassWasapi.BASS_WASAPI_GetDeviceInfo(nDevNo);
                    BASS_WASAPI_INFO wasapiInfo = BassWasapi.BASS_WASAPI_GetInfo();
                    int samplingBytes = 2 * wasapiInfo.chans;
                    switch (wasapiInfo.format)
                    {
                        case BASSWASAPIFormat.BASS_WASAPI_FORMAT_8BIT:
                            samplingBytes = wasapiInfo.chans;
                            break;
                        case BASSWASAPIFormat.BASS_WASAPI_FORMAT_16BIT:
                            samplingBytes = 2 * wasapiInfo.chans;
                            break;
                        case BASSWASAPIFormat.BASS_WASAPI_FORMAT_24BIT:
                            samplingBytes = 3 * wasapiInfo.chans;
                            break;
                        case BASSWASAPIFormat.BASS_WASAPI_FORMAT_32BIT:
                            samplingBytes = 4 * wasapiInfo.chans;
                            break;
                        case BASSWASAPIFormat.BASS_WASAPI_FORMAT_FLOAT:
                            samplingBytes = 4 * wasapiInfo.chans;
                            break;
                    }
                    int onescByte = samplingBytes * wasapiInfo.freq;
                    RBufferSize = (long)(wasapiInfo.buflen * 1000f / onescByte);
                    RDelaySize = 0L;
                }
                else
                {
                    RDelaySize = 0L;
                    BASS_WASAPI_DEVICEINFO devInfo = BassWasapi.BASS_WASAPI_GetDeviceInfo(BassWasapi.BASS_WASAPI_GetDevice());
                }
                BASS_WASAPI_INFO info = BassWasapi.BASS_WASAPI_GetInfo();
                Mixer = BassMix.BASS_Mixer_StreamCreate(info.freq, info.chans, BASSFlag.BASS_SAMPLE_FLOAT | BASSFlag.BASS_STREAM_PRESCAN | BASSFlag.BASS_STREAM_DECODE);
                if (Mixer == 0)
                {
                    BASSError errcode = Bass.BASS_ErrorGetCode();
                    BassWasapi.BASS_WASAPI_Free();
                    Free();
                    throw new Exception($"BASSミキサーの作成に失敗しました。[{errcode}]");
                }
                _mixer_deviceOut = BassMix.BASS_Mixer_StreamCreate(info.freq, info.chans, BASSFlag.BASS_SAMPLE_FLOAT | BASSFlag.BASS_STREAM_PRESCAN | BASSFlag.BASS_STREAM_DECODE);
                if (_mixer_deviceOut == 0)
                {
                    BASSError errcode2 = Bass.BASS_ErrorGetCode();
                    Free();
                    throw new Exception($"BASSミキサー（出力用）の作成に失敗しました。[{errcode2}]");
                }
                if (!BassMix.BASS_Mixer_StreamAddChannel(_mixer_deviceOut, Mixer, BASSFlag.BASS_DEFAULT))
                {
                    BASSError errcode3 = Bass.BASS_ErrorGetCode();
                    BassWasapi.BASS_WASAPI_Free();
                    Free();
                    throw new Exception($"BASSミキサーの接続に失敗しました。[{errcode3}]");
                }
                BassWasapi.BASS_WASAPI_Start();
                return;
            }
            BASSError errcode4 = Bass.BASS_ErrorGetCode();
            Free();
            throw new Exception($"BASS (WASAPI) の初期化に失敗しました。[{errcode4}]");
        }

        /// <summary>
        /// マスターボリュームを設定します（0.0～1.0）
        /// </summary>
        /// <param name="value"></param>
        public static void SetMasterVolume(float value)
        {
            Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_GVOL_STREAM, (int)((double)value * 10000.0));
            Bass.BASS_ChannelSetAttribute(Mixer, BASSAttribute.BASS_ATTRIB_VOL, value);
            Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_GVOL_STREAM, (int)(value * 10000.0 / 255.0));
        }

        private static int WasapiProc(nint buffer, int length, nint user)
        {
            int bytesRead = Bass.BASS_ChannelGetData(_mixer_deviceOut, buffer, length);
            if (bytesRead == -1)
            {
                bytesRead = 0;
            }
            return bytesRead;
        }
    }
}
