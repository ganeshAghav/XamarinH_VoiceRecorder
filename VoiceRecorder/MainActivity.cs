using Android.App;
using Android.Widget;
using Android.OS;
using Android.Support.V7.App;
using Plugin.AudioRecorder;
using System;
using System.Threading.Tasks;
using Android.Support.V4.Content;
using Android;
using Android.Content.PM;
using Android.Support.V4.App;

namespace VoiceRecorder
{
    [Activity(Label = "VoiceRecorder", MainLauncher = true)]
    public class MainActivity : Activity
    {
        AudioRecorderService recorder;
        AudioPlayer player;

        Button recordButton;
        Button playButton;


        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            recordButton = FindViewById<Button>(Resource.Id.recordButton);
            playButton = FindViewById<Button>(Resource.Id.playButton);

            recordButton.Click += RecordButton_Click;
            playButton.Click += PlayButton_Click;

            //if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.RecordAudio) != Permission.Granted)
            //{
            //    ActivityCompat.RequestPermissions(this, new String[] { Manifest.Permission.RecordAudio }, 1);
            //}
        }

        protected override void OnStart()
        {
            base.OnStart();

            try
            {
                recorder = new AudioRecorderService { };
                //recorder = new AudioRecorderService
                //{
                //    StopRecordingOnSilence = true, //will stop recording after 2 seconds (default)
                //    StopRecordingAfterTimeout = true,  //stop recording after a max timeout (defined below)
                //    TotalAudioTimeout = TimeSpan.FromSeconds(15) //audio will stop recording after 15 seconds
                //};

                //alternative event-based API can be used here in lieu of the returned recordTask used below
                recorder.AudioInputReceived += Recorder_AudioInputReceived;

                player = new AudioPlayer();
                player.FinishedPlaying += Player_FinishedPlaying;

                recordButton.Enabled = true;
            }
            catch(Exception er)
            {
                Toast.MakeText(this, er.ToString(), ToastLength.Long);
            }
        }

        async void RecordButton_Click(object sender, EventArgs e)
        {
            await RecordAudio();
        }

        async Task RecordAudio()
        {
            try
            {
                if (!recorder.IsRecording)
                {
                    var checkTimeout = FindViewById<CheckBox>(Resource.Id.checkBoxTimeout);
                    recorder.StopRecordingOnSilence = checkTimeout.Checked;

                    recordButton.Enabled = false;
                    playButton.Enabled = false;

                    //the returned Task here will complete once recording is finished
                    var recordTask = await recorder.StartRecording();

                    recordButton.Text = "Stop";
                    recordButton.Enabled = true;

                    var audioFile = await recordTask;

                    //audioFile will contain the path to the recorded audio file

                    recordButton.Text = "Record";

                    playButton.Enabled = !string.IsNullOrEmpty(audioFile);
                }
                else
                {
                    recordButton.Enabled = false;

                    await recorder.StopRecording();

                    recordButton.Text = "Record";
                    recordButton.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                //blow up the app!
                throw ex;
            }
        }

        void Recorder_AudioInputReceived(object sender, string audioFile)
        {
            RunOnUiThread(() =>
            {
                recordButton.Text = "Record";

                playButton.Enabled = !string.IsNullOrEmpty(audioFile);
            });
        }

        void PlayButton_Click(object sender, EventArgs e)
        {
            PlayRecordedAudio();
        }

        void PlayRecordedAudio()
        {
            try
            {
                var filePath = recorder.GetAudioFilePath();

                if (filePath != null)
                {
                    recordButton.Enabled = false;
                    playButton.Enabled = false;

                    player.Play(filePath);
                }
            }
            catch (Exception ex)
            {
                //blow up the app!
                throw ex;
            }
        }

        private void Player_FinishedPlaying(object sender, EventArgs e)
        {
            recordButton.Enabled = true;
            playButton.Enabled = true;
        }

    }
}

