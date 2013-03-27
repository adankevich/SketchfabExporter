using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using SketchfabPublisher.Properties;

namespace SketchfabPublisher
{
    public partial class ParametersForm : Form
    {
        #region Properties

        public string ModelPath
        {
            get;
            set;
        }

        public string ImagePath
        {
            get;
            set;
        }

        public string ModelTitle
        {
            get
            {
                return txtTitle.Text;
            }

            set
            {
                txtTitle.Text = value;
            }
        }

        public string FileExtension
        {
            get
            {
                return null;// lblExtension.Text;
            }

            private set
            {
                //lblExtension.Text = value;
            }
        }

        public string ModelDescription
        {
            get
            {
                return txtDescription.Text;
            }

            set
            {
                txtDescription.Text = value;
            }
        }

        public string ModelTags
        {
            get
            {
                return txtTags.Text;
            }

            set
            {
                txtTags.Text = value;
            }
        }

        public string APIToken
        {
            get
            {
                return txtToken.Text;
            }

            set
            {
                txtToken.Text = value;
            }
        }

        public string Source
        {
            get;
            set;
        }

        public bool ToSaveSummaryInfo
        {
            get
            {
                return chkSaveSummary.Checked;
            }
        }

        #endregion

        public ParametersForm()
        {
            InitializeComponent();
            
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.txtTitle.Select();
            this.APIToken = Settings.Default.APIToken;
        }

        public ParametersForm(
            string modelPath,
            string modelName = null,
            string description = null,
            string tags = null,
            string token = null,
            string imagePath = null,
            string source = null)
            : this()
        {
            if (!String.IsNullOrEmpty(imagePath) && File.Exists(imagePath))
            {
                //picThumb.Image = System.Drawing.Image.FromFile(imagePath);
                this.ImagePath = imagePath;
            }

            this.ModelPath = modelPath;
            FileExtension = Path.GetExtension(modelPath);
            ModelTitle = modelName;
            ModelDescription = description;
            ModelTags = tags;
            APIToken = token;
            Source = source;
        }

        private bool checkInput()
        {
            bool isOK = true;

            isOK &= !String.IsNullOrWhiteSpace(txtTitle.Text);
            isOK &= !String.IsNullOrWhiteSpace(txtToken.Text);

            return isOK;
        }

        private void btnPublish_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;

            if (!checkInput())
            {
                this.DialogResult = System.Windows.Forms.DialogResult.None;
                return;
            }

            string warn = "", error = "";
            var id = Publisher.Publish(
                modelPath: this.ModelPath,
                modelName: this.ModelTitle,
                description: this.ModelDescription,
                tags: this.ModelTags,
                token: this.APIToken,
                imagePath: this.ImagePath,
                source: this.Source,
                warn: ref warn,
                error: ref error
            );

            if (!String.IsNullOrWhiteSpace(id))
            {
                System.Diagnostics.Process.Start("https://sketchfab.com/show/" + id);
            }
            else
            {
                MessageBox.Show(
                    "Failed to upload the model to Sketchfab.com." + Environment.NewLine +
                    (String.IsNullOrWhiteSpace (error) ? "" : "Error: " + error) +
                    (String.IsNullOrWhiteSpace(warn) ? "" : "Warn: " + warn),
                    "Sketchfab Exporter");
            }

            if (false == String.IsNullOrWhiteSpace(APIToken))
            {
                Settings.Default.APIToken = APIToken;
                Settings.Default.Save();
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
            Cursor.Current = Cursors.Default;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var rect = this.ClientRectangle;
            rect.Inflate(-1, -1);
            ControlPaint.DrawBorder(e.Graphics, rect, Color.DarkBlue, ButtonBorderStyle.Solid);
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("http://www.sketchfab.com");
        }
    }
}
