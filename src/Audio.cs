using System.IO;
using Un4seen.Bass;
using Un4seen.Bass.AddOn.Fx;
using Un4seen.Bass.AddOn.Mix;

namespace Sharp
{
    /// <summary>
    /// BASSライブラリを用いた音声再生と制御を行うクラス
    /// </summary>

    public class Audio
    {
        // 各Audioインスタンスが持つデータ
        private int _handle = -1;
        private int _equalizer;
        private int _pan;
        private float _pitch;
        private float _volume = 1f;
        private float _ratio = 1f;
        private float _freq;
        private long _bytes;
        private BASSChannelType _ctype;
        private double? _loopStartSec;
        private double? _loopEndSec;
        private List<(int Handle, long Bytes)> _activeHandles = new();

        public string FileName { get; set; }
        public bool IsEnable { get; set; }
        public bool Loop { get; set; }

        /// <summary>
        /// Audioインスタンスを生成
        /// </summary>
        public Audio(string fileName, bool loop = false, double? loopStartSec = null, double? loopEndSec = null)
        {
            FileName = fileName.Replace("\\", "/");
            Loop = loop;
            _loopStartSec = loopStartSec;
            _loopEndSec = loopEndSec;
            AudioManager.Register(this);
        }

        ~Audio()
        {
            Dispose();
        }

        /// <summary>
        /// 再生中かどうかを取得します。
        /// </summary>
        public bool IsPlaying
        {
            get
            {
                bool isPlaying = BassMix.BASS_Mixer_ChannelIsActive(_handle) == BASSActive.BASS_ACTIVE_PLAYING;
                return isPlaying && BassMix.BASS_Mixer_ChannelGetPosition(_handle) < _bytes;
            }
        }

        /// <summary>
        /// 左右のパン位置（-1.0 ～ 1.0）
        /// </summary>
        public int Pan
        {
            get => _pan;
            set
            {
                _pan = value;
                Bass.BASS_ChannelSetAttribute(_handle, BASSAttribute.BASS_ATTRIB_PAN, _pan);
            }
        }

        /// <summary>
        /// ピッチ（±範囲）
        /// </summary>
        public float Pitch
        {
            get => _pitch;
            set
            {
                _pitch = float.IsNaN(value) || float.IsInfinity(value) ? 0f : value;
                Bass.BASS_ChannelSetAttribute(_handle, BASSAttribute.BASS_ATTRIB_TEMPO_PITCH, _pitch);
            }
        }

        /// <summary>
        /// 音量（0.0～1.0）
        /// </summary>
        public float Volume
        {
            get => _volume;
            set
            {
                _volume = value;
                if (float.IsNaN(_volume) || float.IsInfinity(_volume))
                {
                    _volume = 0f;
                }
                Bass.BASS_ChannelSetAttribute(_handle, BASSAttribute.BASS_ATTRIB_VOL, _volume);
            }
        }

        /// <summary>
        /// 再生中の時間（秒）
        /// </summary>
        public double Time
        {
            get => Bass.BASS_ChannelBytes2Seconds(_handle, BassMix.BASS_Mixer_ChannelGetPosition(_handle));
            set => BassMix.BASS_Mixer_ChannelSetPosition(_handle, Bass.BASS_ChannelSeconds2Bytes(_handle, value));
        }

        /// <summary>
        /// 全体の長さ（秒）
        /// </summary>
        public double TotalTime => Bass.BASS_ChannelBytes2Seconds(_handle, Bass.BASS_ChannelGetLength(_handle));

        /// <summary>
        /// 再生速度（1.0が等倍）
        /// </summary>
        public float PlaySpeed
        {
            get => _ratio;
            set
            {
                _ratio = value;
                Bass.BASS_ChannelSetAttribute(_handle, BASSAttribute.BASS_ATTRIB_FREQ, _ratio * _freq);
            }
        }

        /// <summary>
        /// 音声の再生を開始する。(複数回呼び出した場合重ねて再生します)
        /// </summary>
        public void Play(bool playFromBegin = true)
        {
            Pan = _pan;
            Pitch = _pitch;
            PlaySpeed = _ratio;
            Volume = _volume;

            Load();
            _activeHandles.Add((_handle, _bytes));
            AudioManager.ListProcOverlaps.Add((_handle, _bytes));

            if (playFromBegin)
            {
                BassMix.BASS_Mixer_ChannelSetPosition(_handle, 0L);
            }
            // 再生開始時にループ開始地点にジャンプ
            if (Loop && _loopStartSec.HasValue)
            {
                long loopStartBytes = Bass.BASS_ChannelSeconds2Bytes(_handle, _loopStartSec.Value);
                BassMix.BASS_Mixer_ChannelSetPosition(_handle, loopStartBytes);
            }

            BassMix.BASS_Mixer_ChannelPlay(_handle);
        }

        /// <summary>
        /// 音声の再生を開始する。(重ねずに再生します)
        /// </summary>
        public void PlayOneShot(bool playFromBegin = true)
        {
            Pan = _pan;
            Pitch = _pitch;
            PlaySpeed = _ratio;
            Volume = _volume;

            Dispose();
            Load();
            _activeHandles.Add((_handle, _bytes));

            if (playFromBegin)
            {
                BassMix.BASS_Mixer_ChannelSetPosition(_handle, 0L);
            }
            // 再生開始時にループ開始地点にジャンプ
            if (Loop && _loopStartSec.HasValue)
            {
                long loopStartBytes = Bass.BASS_ChannelSeconds2Bytes(_handle, _loopStartSec.Value);
                BassMix.BASS_Mixer_ChannelSetPosition(_handle, loopStartBytes);
            }

            BassMix.BASS_Mixer_ChannelPlay(_handle);
        }

        /// <summary>
        /// 再生中の音声を停止。
        /// </summary>
        public void Stop()
        {
            BassMix.BASS_Mixer_ChannelPause(_handle);
        }

        /// <summary>
        /// ループすべきタイミングかを判定して必要に応じてシークする(自動的に実行されるので、Sharpを利用する側は使わなくていい)
        /// </summary>
        public void CheckAndLoop()
        {
            foreach (var (handle, bytes) in _activeHandles.ToList())
            {
                if (_loopStartSec.HasValue && _loopEndSec.HasValue)
                {
                    long pos = Bass.BASS_ChannelGetPosition(handle);
                    double time = Bass.BASS_ChannelBytes2Seconds(handle, pos);
                    if (time >= _loopEndSec.Value)
                    {
                        long loopStartPos = Bass.BASS_ChannelSeconds2Bytes(handle, _loopStartSec.Value);
                        Bass.BASS_ChannelSetPosition(handle, loopStartPos);
                    }
                }

                if (BassMix.BASS_Mixer_ChannelIsActive(handle) == BASSActive.BASS_ACTIVE_STOPPED)
                {
                    _activeHandles.Remove((handle, bytes));
                }
            }
        }

        #region [private]
        private void Load(bool isClear = false, float[] _gains = null)
        {
            _handle = ((!File.Exists(FileName)) ? Bass.BASS_StreamCreateFile(FileName.Replace(".ogg", ".wav"), 0L, 0L, BASSFlag.BASS_SAMPLE_FLOAT | BASSFlag.BASS_STREAM_DECODE) : Bass.BASS_StreamCreateFile(FileName, 0L, 0L, BASSFlag.BASS_SAMPLE_FLOAT | BASSFlag.BASS_STREAM_DECODE));
            if (_handle != 0)
            {
                BASS_CHANNELINFO info = Bass.BASS_ChannelGetInfo(_handle);
                IsEnable = true;
                _ctype = info.ctype;
                _bytes = Bass.BASS_ChannelGetLength(_handle);
                _handle = BassFx.BASS_FX_TempoCreate(_handle, BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_SAMPLE_OVER_VOL);
                if (_gains == null)
                {
                    _gains = new float[10] { 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f };
                }
                else
                {
                    Attach(_handle, _gains);
                }
                Bass.BASS_ChannelGetAttribute(_handle, BASSAttribute.BASS_ATTRIB_FREQ, ref _freq);
                if (Loop) Bass.BASS_ChannelFlags(_handle, BASSFlag.BASS_SAMPLE_LOOP, BASSFlag.BASS_SAMPLE_LOOP);
                BassMix.BASS_Mixer_StreamAddChannel(AudioManager.Mixer, _handle, BASSFlag.BASS_STREAM_STATUS | BASSFlag.BASS_SPEAKER_FRONT);
                BassMix.BASS_Mixer_ChannelPause(_handle);
            }
        }

        private void Dispose()
        {
            BassMix.BASS_Mixer_ChannelRemove(_handle);
            Bass.BASS_StreamFree(_handle);
            _handle = -1;
            IsEnable = false;
        }

        private void Attach(int stream, float[] _gains)
        {
            _equalizer = Bass.BASS_ChannelSetFX(stream, BASSFXType.BASS_FX_BFX_PEAKEQ, 0);
            BASS_BFX_PEAKEQ par = new BASS_BFX_PEAKEQ
            {
                fBandwidth = 0.5f,
                fQ = 0f,
                lChannel = BASSFXChan.BASS_BFX_CHANALL
            };
            for (int index = 0; index < _gains.Length; index++)
            {
                par.fGain = _gains[index];
                par.lBand = index;
                par.fCenter = AudioManager.GainCenters[index];
                Bass.BASS_FXSetParameters(_equalizer, par);
            }
        }
        #endregion
    }
}
