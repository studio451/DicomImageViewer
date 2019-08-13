using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace Avi {
    public class AviPlayer {

        private VideoStream videoStream;
        private PictureBox picDisplay;
        private Control ctlFrameIndexFeedback;
        private int millisecondsPerFrame;
        private bool isRunning;
        private int currentFrameIndex;
        private Bitmap currentBitmap;

        private delegate void SimpleDelegate();
        public event EventHandler Stopped;

        public bool IsRunning {
            get { return isRunning; }
        }

        public AviPlayer(VideoStream videoStream, PictureBox picDisplay, Control ctlFrameIndexFeedback) {
            this.videoStream = videoStream;
            this.picDisplay = picDisplay;
            this.ctlFrameIndexFeedback = ctlFrameIndexFeedback;
            this.isRunning = false;
        }

        public void Start() {
            isRunning = true;
            millisecondsPerFrame = (int)(1000 / videoStream.FrameRate);
            Thread thread = new Thread(new ThreadStart(Run));
            thread.Start();
        }

        private void Run() {
            videoStream.GetFrameOpen();

            for (currentFrameIndex = 0; (currentFrameIndex < videoStream.CountFrames) && isRunning; currentFrameIndex++) {
                currentBitmap = videoStream.GetBitmap(currentFrameIndex);
                picDisplay.Invoke(new SimpleDelegate(SetDisplayPicture));
                picDisplay.Invoke(new SimpleDelegate(picDisplay.Refresh));

                if (ctlFrameIndexFeedback != null) {
                    ctlFrameIndexFeedback.Invoke(new SimpleDelegate(SetLabelText));
                }

                Thread.Sleep(millisecondsPerFrame);
            }

            videoStream.GetFrameClose();
            isRunning = false;

            if (Stopped != null) {
                Stopped(this, EventArgs.Empty);
            }
        }

        private void SetDisplayPicture() {
            picDisplay.Image = currentBitmap;
        }

        private void SetLabelText() {
            ctlFrameIndexFeedback.Text = currentFrameIndex.ToString();
        }

        public void Stop() {
            isRunning = false;
        }
    }
}
