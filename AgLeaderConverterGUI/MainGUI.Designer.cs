namespace AgLeaderConverterGUI
{
    partial class MainGUI
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainGUI));
            this.btnStartConverter = new System.Windows.Forms.Button();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.btnChooseInput = new System.Windows.Forms.Button();
            this.btnChooseOutput = new System.Windows.Forms.Button();
            this.lblInputDirectory = new System.Windows.Forms.Label();
            this.lblOutputDirectory = new System.Windows.Forms.Label();
            this.pbFiles = new System.Windows.Forms.ProgressBar();
            this.btnHelp = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnStartConverter
            // 
            this.btnStartConverter.Location = new System.Drawing.Point(12, 66);
            this.btnStartConverter.Name = "btnStartConverter";
            this.btnStartConverter.Size = new System.Drawing.Size(75, 23);
            this.btnStartConverter.TabIndex = 0;
            this.btnStartConverter.Text = "Start";
            this.btnStartConverter.UseVisualStyleBackColor = true;
            this.btnStartConverter.Click += new System.EventHandler(this.btnStartConverter_Click);
            // 
            // btnChooseInput
            // 
            this.btnChooseInput.Location = new System.Drawing.Point(12, 8);
            this.btnChooseInput.Name = "btnChooseInput";
            this.btnChooseInput.Size = new System.Drawing.Size(75, 23);
            this.btnChooseInput.TabIndex = 1;
            this.btnChooseInput.Text = "Choose...";
            this.btnChooseInput.UseVisualStyleBackColor = true;
            this.btnChooseInput.Click += new System.EventHandler(this.btnChooseInput_Click);
            // 
            // btnChooseOutput
            // 
            this.btnChooseOutput.Location = new System.Drawing.Point(12, 37);
            this.btnChooseOutput.Name = "btnChooseOutput";
            this.btnChooseOutput.Size = new System.Drawing.Size(75, 23);
            this.btnChooseOutput.TabIndex = 2;
            this.btnChooseOutput.Text = "Choose...";
            this.btnChooseOutput.UseVisualStyleBackColor = true;
            this.btnChooseOutput.Click += new System.EventHandler(this.btnChooseOutput_Click);
            // 
            // lblInputDirectory
            // 
            this.lblInputDirectory.AutoSize = true;
            this.lblInputDirectory.Location = new System.Drawing.Point(93, 13);
            this.lblInputDirectory.Name = "lblInputDirectory";
            this.lblInputDirectory.Size = new System.Drawing.Size(130, 13);
            this.lblInputDirectory.TabIndex = 3;
            this.lblInputDirectory.Text = "Input Directory (*.csv files)";
            // 
            // lblOutputDirectory
            // 
            this.lblOutputDirectory.AutoSize = true;
            this.lblOutputDirectory.Location = new System.Drawing.Point(93, 42);
            this.lblOutputDirectory.Name = "lblOutputDirectory";
            this.lblOutputDirectory.Size = new System.Drawing.Size(181, 13);
            this.lblOutputDirectory.TabIndex = 4;
            this.lblOutputDirectory.Text = "Output Directory (AgOpenGPS fields)";
            // 
            // pbFiles
            // 
            this.pbFiles.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pbFiles.Location = new System.Drawing.Point(0, 100);
            this.pbFiles.Name = "pbFiles";
            this.pbFiles.Size = new System.Drawing.Size(303, 23);
            this.pbFiles.TabIndex = 8;
            // 
            // btnHelp
            // 
            this.btnHelp.Location = new System.Drawing.Point(228, 71);
            this.btnHelp.Name = "btnHelp";
            this.btnHelp.Size = new System.Drawing.Size(75, 23);
            this.btnHelp.TabIndex = 9;
            this.btnHelp.Text = "Help";
            this.btnHelp.UseVisualStyleBackColor = true;
            this.btnHelp.Click += new System.EventHandler(this.btnHelp_Click);
            // 
            // MainGUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(303, 123);
            this.Controls.Add(this.btnHelp);
            this.Controls.Add(this.pbFiles);
            this.Controls.Add(this.lblOutputDirectory);
            this.Controls.Add(this.lblInputDirectory);
            this.Controls.Add(this.btnChooseOutput);
            this.Controls.Add(this.btnChooseInput);
            this.Controls.Add(this.btnStartConverter);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainGUI";
            this.Text = "AgLeader - AgOpenGPS Converter";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnStartConverter;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.Button btnChooseInput;
        private System.Windows.Forms.Button btnChooseOutput;
        private System.Windows.Forms.Label lblInputDirectory;
        private System.Windows.Forms.Label lblOutputDirectory;
        private System.Windows.Forms.ProgressBar pbFiles;
        private System.Windows.Forms.Button btnHelp;
    }
}

