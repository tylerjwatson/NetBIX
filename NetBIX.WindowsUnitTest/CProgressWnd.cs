using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NetBIX.WindowsUnitTest {
    public partial class CProgressWnd : Form {
        public string title;
        long itemsPerSecond = 0;

        System.Windows.Forms.Timer oneSecondTimer = new System.Windows.Forms.Timer();

        public CProgressWnd(string Title) {
            this.title = Title;
            InitializeComponent();
            this.lblTitle.Text = Title;
            this.Text = Title;
            oneSecondTimer.Tick += oneSecondTimer_Tick;
            oneSecondTimer.Interval = 1000;
           
        }

        void oneSecondTimer_Tick(object sender, EventArgs e) {
            RunOnUIThread(() => {
                lblSummary.Text = string.Format("{0}/{1}", pgBar.Value, pgBar.Maximum);
                lblItemsSec.Text = itemsPerSecond + " items/sec";
            });

            Interlocked.Exchange(ref itemsPerSecond, 0);
        }

        /// <summary>
        /// Runs a method on the UI thread this form was created from.
        /// </summary>
        public void RunOnUIThread(Action @delegate) {
            if (this.InvokeRequired) {
                this.Invoke(@delegate);
            } else {
                @delegate();
            }
        }

        /// <summary>
        /// Runs a method on the UI thread this form was created from.
        /// </summary>
        public T RunOnUIThread<T>(Func<T> @delegate) {
            if (this.InvokeRequired) {
                return (T)this.Invoke(@delegate);
            } else {
                return (T)@delegate();
            }
        }

        private void CProgressWnd_Load(object sender, EventArgs e) {
            oneSecondTimer.Start();
        }

        public void Increment() {
            Interlocked.Increment(ref itemsPerSecond);

            this.ProgressValue++;
        }

        /// <summary>
        /// Gets or sets the progress bar's upper bound
        /// </summary>
        public int ProgressMaximumValue {
            get {
                return this.RunOnUIThread<int>(() => pgBar.Maximum);
            }
            set {
                this.RunOnUIThread(() => {
                    pgBar.Maximum = value;
                });
            }
        }

        /// <summary>
        /// Gets or sets the progress bar's value.
        /// </summary>
        public int ProgressValue {
            get {
                return this.RunOnUIThread<int>(() => pgBar.Value);
            }
            set {
                this.RunOnUIThread(() => {
                    if (value <= pgBar.Maximum) {
                        pgBar.Value = value;
                    }
                });
            }
        }

        private void lblTitle_Click(object sender, EventArgs e) {

        }

        private void CProgressWnd_FormClosing(object sender, FormClosingEventArgs e) {
            oneSecondTimer.Stop();
        }

    }
}
