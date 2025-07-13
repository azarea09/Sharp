using Un4seen.Bass;
using Un4seen.Bass.AddOn.Fx;
using Un4seen.Bass.AddOn.Mix;

namespace SharpFramework.Audio
{
    /// <summary>
    /// 音声ファイルの再生・制御を行うクラス（BASSライブラリ使用）
    /// </summary>
    public class Audio
    {
        private int _handle = -1, _equalizer;
        private float _baseFreq;
        private double _pitch;
        private double _volume = 1.0;
        private double _actualVolume = 1.0;
        private double _ratio = 1.0;
        private double _pan;
        private long _bytes;
        private BASSChannelType _ctype;
        private double? _loopStartSec, _loopEndSec;
        private readonly List<(int Handle, long Bytes)> _activeHandles = new();

        public string FilePath { get; private set; }
        public bool IsEnable { get; private set; }
        public bool Loop { get; set; }

        /// <summary>
        /// 再生中か
        /// </summary>
        public bool IsPlaying =>
            BassMix.BASS_Mixer_ChannelIsActive(_handle) == BASSActive.BASS_ACTIVE_PLAYING &&
            BassMix.BASS_Mixer_ChannelGetPosition(_handle) < _bytes;

        /// <summary>
        /// パン（左右） -1.0～1.0
        /// </summary>
        public double Pan
        {
            get => _pan;
            set
            {
                _pan = value;
                SetChannelAttribute(BASSAttribute.BASS_ATTRIB_PAN, (float)_pan);
            }
        }
        /// <summary>
        /// ピッチ
        /// </summary>
        public double Pitch
        {
            get => _pitch;
            set
            {
                _pitch = Sanitize(value);
                SetChannelAttribute(BASSAttribute.BASS_ATTRIB_TEMPO_PITCH, (float)_pitch);
            }
        }

        /// <summary>
        /// 音量（0.0～）
        /// </summary>
        public double Volume
        {
            get => _volume;
            set
            {
                // 人間の耳に近い対数的なスケールで音量を変換
                _volume = Sanitize(value);
                _actualVolume = Sanitize((float)Math.Pow(value, 0.2));
                SetChannelAttribute(BASSAttribute.BASS_ATTRIB_VOL, (float)_actualVolume);
            }
        }

        /// <summary>
        /// 再生速度（等倍＝1.0）
        /// </summary>
        public double PlaySpeed
        {
            get => _ratio;
            set
            {
                _ratio = Sanitize(value);
                SetChannelAttribute(BASSAttribute.BASS_ATTRIB_FREQ, _baseFreq * (float)_ratio);
            }
        }

        /// <summary>
        /// 再生時間（秒）
        /// </summary>
        public double Time
        {
            get => Bass.BASS_ChannelBytes2Seconds(_handle, BassMix.BASS_Mixer_ChannelGetPosition(_handle));
            set => BassMix.BASS_Mixer_ChannelSetPosition(_handle, Bass.BASS_ChannelSeconds2Bytes(_handle, value));
        }

        /// <summary>
        /// 音声の総再生時間（秒）
        /// </summary>
        public double TotalTime => Bass.BASS_ChannelBytes2Seconds(_handle, Bass.BASS_ChannelGetLength(_handle));

        /// <summary>
        /// 音声を再生するためのクラス
        /// </summary>
        /// <param name="filePath">音声のあるファイルパス</param>
        /// <param name="loop">ループするかどうか</param>
        /// <param name="loopStartSec">ループする開始地点 (デフォルト0.0)</param>
        /// <param name="loopEndSec">ループする終了地点 (デフォルト音源の最後)</param>
        public Audio(string filePath, bool loop = false, double? loopStartSec = null, double? loopEndSec = null)
        {
            FilePath = filePath.Replace("\\", "/");
            Loop = loop;
            _loopStartSec = loopStartSec;
            _loopEndSec = loopEndSec;
            AudioManager.Register(this);
        }

        ~Audio() => Dispose();

        /// <summary>
        /// 音声再生（重ねて再生）
        /// </summary>
        public void Play(bool playFromBegin = true)
        {
            Load();
            _activeHandles.Add((_handle, _bytes));
            AudioManager.ListProcOverlaps.Add((_handle, _bytes));
            if (playFromBegin) BassMix.BASS_Mixer_ChannelSetPosition(_handle, 0L);
            if (Loop && _loopStartSec.HasValue)
                BassMix.BASS_Mixer_ChannelSetPosition(_handle, Bass.BASS_ChannelSeconds2Bytes(_handle, _loopStartSec.Value));
            BassMix.BASS_Mixer_ChannelPlay(_handle);
        }

        /// <summary>
        /// 音声再生（1つだけ再生）
        /// </summary>
        public void PlayOneShot(bool playFromBegin = true)
        {
            Dispose();
            Load();
            _activeHandles.Add((_handle, _bytes));
            if (playFromBegin) BassMix.BASS_Mixer_ChannelSetPosition(_handle, 0L);
            if (Loop && _loopStartSec.HasValue)
                BassMix.BASS_Mixer_ChannelSetPosition(_handle, Bass.BASS_ChannelSeconds2Bytes(_handle, _loopStartSec.Value));
            BassMix.BASS_Mixer_ChannelPlay(_handle);
        }

        /// <summary>
        /// 停止
        /// </summary>
        public void Stop() => BassMix.BASS_Mixer_ChannelPause(_handle);

        /// <summary>
        /// ループ再生の自動判定 (Sharpを利用する側は使いません)
        /// </summary>
        public void CheckAndLoop()
        {
            foreach (var (handle, bytes) in _activeHandles.ToList())
            {
                if (_loopStartSec.HasValue && _loopEndSec.HasValue)
                {
                    double time = Bass.BASS_ChannelBytes2Seconds(handle, Bass.BASS_ChannelGetPosition(handle));
                    if (time >= _loopEndSec.Value)
                        Bass.BASS_ChannelSetPosition(handle, Bass.BASS_ChannelSeconds2Bytes(handle, _loopStartSec.Value));
                }

                if (BassMix.BASS_Mixer_ChannelIsActive(handle) == BASSActive.BASS_ACTIVE_STOPPED)
                    _activeHandles.Remove((handle, bytes));
            }
        }


        private void Load(float[] gains = null)
        {
            string path = File.Exists(FilePath) ? FilePath : FilePath.Replace(".ogg", ".wav");
            _handle = Bass.BASS_StreamCreateFile(path, 0L, 0L, BASSFlag.BASS_SAMPLE_FLOAT | BASSFlag.BASS_STREAM_DECODE);
            if (_handle == 0) return;

            _bytes = Bass.BASS_ChannelGetLength(_handle);
            _handle = BassFx.BASS_FX_TempoCreate(_handle, BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_SAMPLE_OVER_VOL);
            _ctype = Bass.BASS_ChannelGetInfo(_handle).ctype;

            SetChannelAttribute(BASSAttribute.BASS_ATTRIB_PAN, (float)_pan);
            SetChannelAttribute(BASSAttribute.BASS_ATTRIB_TEMPO_PITCH, (float)_pitch);
            SetChannelAttribute(BASSAttribute.BASS_ATTRIB_VOL, (float)_actualVolume);
            Bass.BASS_ChannelGetAttribute(_handle, BASSAttribute.BASS_ATTRIB_FREQ, ref _baseFreq);
            SetChannelAttribute(BASSAttribute.BASS_ATTRIB_FREQ, (float)(_ratio * _baseFreq));

            if (gains != null) Attach(_handle, gains);

            if (Loop)
                Bass.BASS_ChannelFlags(_handle, BASSFlag.BASS_SAMPLE_LOOP, BASSFlag.BASS_SAMPLE_LOOP);

            BassMix.BASS_Mixer_StreamAddChannel(AudioManager.Mixer, _handle, BASSFlag.BASS_STREAM_STATUS | BASSFlag.BASS_SPEAKER_FRONT);
            BassMix.BASS_Mixer_ChannelPause(_handle);
            IsEnable = true;
        }

        private void Dispose()
        {
            BassMix.BASS_Mixer_ChannelRemove(_handle);
            Bass.BASS_StreamFree(_handle);
            _handle = -1;
            IsEnable = false;
        }

        private void Attach(int stream, float[] gains)
        {
            _equalizer = Bass.BASS_ChannelSetFX(stream, BASSFXType.BASS_FX_BFX_PEAKEQ, 0);
            var par = new BASS_BFX_PEAKEQ { fBandwidth = 0.5f, fQ = 0f, lChannel = BASSFXChan.BASS_BFX_CHANALL };
            for (int i = 0; i < gains.Length; i++)
            {
                par.fGain = gains[i];
                par.lBand = i;
                par.fCenter = AudioManager.GainCenters[i];
                Bass.BASS_FXSetParameters(_equalizer, par);
            }
        }

        private void SetChannelAttribute(BASSAttribute attr, float value)
        {
            if (_handle != -1) Bass.BASS_ChannelSetAttribute(_handle, attr, value);
        }

        private double Sanitize(double value) =>
            double.IsNaN(value) || double.IsInfinity(value) ? 0 : value;
    }
}
