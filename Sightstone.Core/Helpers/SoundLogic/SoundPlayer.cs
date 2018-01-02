using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Sightstone.Core.Helpers.SoundLogic
{
    /// <summary>
    /// Helps Sightstone play sounds
    /// </summary>
    public class SoundPlayer : IDisposable
    {
        #region mciFunctions
        /// <summary>
        /// Advanced method of playing audio which allows for more control
        /// </summary>
        /// <param name="strCommand">Pointer to a null-terminated string that specifies an MCI command string</param>
        /// <param name="strReturn">Pointer to a buffer that receives return information. If no return information is needed, this parameter can be <seealso cref=">null"/>.</param>
        /// <param name="iReturnLength">Size, in characters, of the return buffer specified by the lpszReturnString parameter</param>
        /// <param name="hwndCallback">Handle to a callback window if the "notify" flag was specified in the command string</param>
        /// <returns>Returns zero if successful or an error otherwise</returns>
        [DllImport("winmm.dll")]
        private static extern long mciSendString(string strCommand, StringBuilder strReturn, int iReturnLength, IntPtr hwndCallback);
        

        /// <summary>
        /// Plays the audio track
        /// </summary>
        public void Play()
        {
            if (Settings.StartTime < Settings.EndTime && 
                Settings.StartTime >= 0 && 
                PlayLength >= Settings.EndTime)
            {
                mciSendString($"play { FileName } from { Settings.StartTime } to { Settings.EndTime }", null, 0, IntPtr.Zero);
            }
            else
            {
                mciSendString($"play { FileName }", null, 0, IntPtr.Zero);
            }
        }
        //status MediaFile mode

        /// <summary>
        /// Pauses the audio track
        /// </summary>
        public void Pause()
        {

        }

        /// <summary>
        /// Stops the audio track from playing
        /// </summary>
        public void Stop()
        {
            mciSendString($"stop { FileName }", null, 0, IntPtr.Zero);
        }

        /// <summary>
        /// Deletes all variables
        /// </summary>
        public void Dispose()
        {
            //Stop the audio track from playing
            Stop();

            //Delete local variables
            FileName = null;
            FileLocation = null;
            PlayLength = null;
        }
        #endregion mciFuncitons

        /// <summary>
        /// The audio file's name
        /// </summary>
        private string FileName { get; set; }

        /// <summary>
        /// The audio file's location
        /// </summary>
        private string FileLocation { get; set; }

        /// <summary>
        /// How long the audio file is
        /// </summary>
        private ulong? PlayLength { get; set; }

        private SoundPlayerSettings Settings { get; set; }
        
        public SoundPlayer(string fileLocation)
        {
            Initialize(fileLocation, 
                new SoundPlayerSettings
                {
                    LoopAudio = false
                }, 
                true);
        }

        public SoundPlayer(string fileLocation, bool playAfterInitialize)
        {
            Initialize(fileLocation,
                new SoundPlayerSettings
                {
                    LoopAudio = false
                }, 
                playAfterInitialize);
        }

        public SoundPlayer(string fileLocation, SoundPlayerSettings settings)
        {
            Initialize(fileLocation, settings, true);
        }

        public SoundPlayer(string fileLocation, SoundPlayerSettings settings, bool playAfterInitialize)
        {
            Initialize(fileLocation, settings, playAfterInitialize);
        }

        private void Initialize(string fileLocation, SoundPlayerSettings settings, bool playAfterInitialize)
        {
            string fileType;
            switch (fileLocation.Split('.')[1].ToLower())
            {
                case "mp3":
                    fileType = "mpegvideo";
                    break;
                case "wav":
                    fileType = "waveaudio";
                    break;
                default:
                    throw new NotImplementedException($"the fileformat { fileLocation.Split('.')[1].ToLower() }, is not implemented");
            }
            FileLocation = fileLocation;
            FileName = Path.GetFileNameWithoutExtension(fileLocation);
            mciSendString($"open \"{ fileLocation }\" type { fileType } alias { FileName }", null, 0, IntPtr.Zero);

            //Get the length of the song
            StringBuilder sb = new StringBuilder(128);
            mciSendString($"status { FileName } length", sb, 128, IntPtr.Zero);
            PlayLength = Convert.ToUInt64(sb.ToString());
        }
    }

    public class SoundPlayerSettings
    {
        /// <summary>
        /// Do you want to loop the audio until
        /// </summary>
        public bool LoopAudio { get; set; }

        /// <summary>
        /// If set, the audio will loop for these number of times
        /// </summary>
        public int LoopTimes { get; set; }

        /// <summary>
        /// The amount of delay between when the audio track ends and when it starts to play again
        /// </summary>
        public double LoopDelay { get; set; }

        /// <summary>
        /// The time that the audio track starts at
        /// </summary>
        public double StartTime { get; set; }

        /// <summary>
        /// The time that the audio track ends at
        /// </summary>
        public double EndTime { get; set; }
    }

    public enum PlayMode
    {
        notReady,
        paused,
        playing,
        stopped,

        // only certain devices will have the below
        open,
        parked,
        recording,
        seeking
    }
}
