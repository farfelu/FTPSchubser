using System.ComponentModel;
using System.Windows.Forms;
using System.Windows.Shell;
namespace FTPSchubser
{
    partial class frm_main
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.label1 = new System.Windows.Forms.Label();
            this.p_dropzone = new System.Windows.Forms.Panel();
            this.btn_settings = new System.Windows.Forms.Button();
            this.th_upload = new System.ComponentModel.BackgroundWorker();
            this.btn_save = new System.Windows.Forms.Button();
            this.pb_upload = new System.Windows.Forms.ProgressBar();
            this.txt_host = new System.Windows.Forms.TextBox();
            this.txt_folder = new System.Windows.Forms.TextBox();
            this.txt_user = new System.Windows.Forms.TextBox();
            this.txt_port = new System.Windows.Forms.TextBox();
            this.cb_passive = new System.Windows.Forms.CheckBox();
            this.txt_password = new System.Windows.Forms.MaskedTextBox();
            this.lbl_host = new System.Windows.Forms.Label();
            this.lbl_user = new System.Windows.Forms.Label();
            this.lbl_password = new System.Windows.Forms.Label();
            this.lbl_folder = new System.Windows.Forms.Label();
            this.lbl_preview = new System.Windows.Forms.Label();
            this.t_slider = new System.Windows.Forms.Timer(this.components);
            this.cb_clip = new System.Windows.Forms.CheckBox();
            this.cb_shorten = new System.Windows.Forms.CheckBox();
            this.th_shorten = new System.ComponentModel.BackgroundWorker();
            this.rdb_bitly = new System.Windows.Forms.RadioButton();
            this.rdb_googl = new System.Windows.Forms.RadioButton();
            this.label2 = new System.Windows.Forms.Label();
            this.txt_url = new System.Windows.Forms.TextBox();
            this.lbl_urlpreview = new System.Windows.Forms.Label();
            this.cb_overwrite = new System.Windows.Forms.CheckBox();
            this.p_dropzone.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Font = new System.Drawing.Font("Segoe UI Light", 27.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(0, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(258, 95);
            this.label1.TabIndex = 0;
            this.label1.Text = "dropzone";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // p_dropzone
            // 
            this.p_dropzone.AllowDrop = true;
            this.p_dropzone.BackColor = System.Drawing.Color.White;
            this.p_dropzone.Controls.Add(this.label1);
            this.p_dropzone.Location = new System.Drawing.Point(3, 3);
            this.p_dropzone.Name = "p_dropzone";
            this.p_dropzone.Size = new System.Drawing.Size(258, 95);
            this.p_dropzone.TabIndex = 1;
            this.p_dropzone.DragDrop += new System.Windows.Forms.DragEventHandler(this.p_dropzone_DragDrop);
            this.p_dropzone.DragEnter += new System.Windows.Forms.DragEventHandler(this.p_dropzone_DragEnter);
            // 
            // btn_settings
            // 
            this.btn_settings.Location = new System.Drawing.Point(3, 104);
            this.btn_settings.Name = "btn_settings";
            this.btn_settings.Size = new System.Drawing.Size(75, 23);
            this.btn_settings.TabIndex = 0;
            this.btn_settings.Text = "Settings";
            this.btn_settings.UseVisualStyleBackColor = true;
            this.btn_settings.Click += new System.EventHandler(this.btn_settings_Click);
            // 
            // th_upload
            // 
            this.th_upload.WorkerReportsProgress = true;
            this.th_upload.DoWork += new System.ComponentModel.DoWorkEventHandler(this.th_upload_DoWork);
            this.th_upload.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.th_upload_ProgressChanged);
            this.th_upload.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.th_upload_RunWorkerCompleted);
            // 
            // btn_save
            // 
            this.btn_save.Location = new System.Drawing.Point(186, 402);
            this.btn_save.Name = "btn_save";
            this.btn_save.Size = new System.Drawing.Size(75, 23);
            this.btn_save.TabIndex = 13;
            this.btn_save.Text = "Save";
            this.btn_save.UseVisualStyleBackColor = true;
            this.btn_save.Click += new System.EventHandler(this.btn_save_Click);
            // 
            // pb_upload
            // 
            this.pb_upload.Location = new System.Drawing.Point(82, 104);
            this.pb_upload.Name = "pb_upload";
            this.pb_upload.Size = new System.Drawing.Size(177, 23);
            this.pb_upload.TabIndex = 4;
            // 
            // txt_host
            // 
            this.txt_host.Location = new System.Drawing.Point(66, 147);
            this.txt_host.Name = "txt_host";
            this.txt_host.Size = new System.Drawing.Size(100, 22);
            this.txt_host.TabIndex = 1;
            this.txt_host.TextChanged += new System.EventHandler(this.update_preview);
            // 
            // txt_folder
            // 
            this.txt_folder.Location = new System.Drawing.Point(66, 225);
            this.txt_folder.Name = "txt_folder";
            this.txt_folder.Size = new System.Drawing.Size(188, 22);
            this.txt_folder.TabIndex = 6;
            this.txt_folder.TextChanged += new System.EventHandler(this.update_preview);
            // 
            // txt_user
            // 
            this.txt_user.Location = new System.Drawing.Point(66, 173);
            this.txt_user.Name = "txt_user";
            this.txt_user.Size = new System.Drawing.Size(100, 22);
            this.txt_user.TabIndex = 3;
            // 
            // txt_port
            // 
            this.txt_port.Location = new System.Drawing.Point(172, 147);
            this.txt_port.Name = "txt_port";
            this.txt_port.Size = new System.Drawing.Size(38, 22);
            this.txt_port.TabIndex = 2;
            this.txt_port.Text = "21";
            this.txt_port.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txt_port.TextChanged += new System.EventHandler(this.update_preview);
            // 
            // cb_passive
            // 
            this.cb_passive.AutoSize = true;
            this.cb_passive.Checked = true;
            this.cb_passive.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cb_passive.Location = new System.Drawing.Point(186, 187);
            this.cb_passive.Name = "cb_passive";
            this.cb_passive.Size = new System.Drawing.Size(52, 17);
            this.cb_passive.TabIndex = 5;
            this.cb_passive.Text = "PASV";
            this.cb_passive.UseVisualStyleBackColor = true;
            // 
            // txt_password
            // 
            this.txt_password.Location = new System.Drawing.Point(66, 199);
            this.txt_password.Name = "txt_password";
            this.txt_password.Size = new System.Drawing.Size(100, 22);
            this.txt_password.TabIndex = 4;
            this.txt_password.UseSystemPasswordChar = true;
            // 
            // lbl_host
            // 
            this.lbl_host.AutoSize = true;
            this.lbl_host.Location = new System.Drawing.Point(7, 150);
            this.lbl_host.Name = "lbl_host";
            this.lbl_host.Size = new System.Drawing.Size(56, 13);
            this.lbl_host.TabIndex = 12;
            this.lbl_host.Text = "Host/Port";
            // 
            // lbl_user
            // 
            this.lbl_user.AutoSize = true;
            this.lbl_user.Location = new System.Drawing.Point(10, 176);
            this.lbl_user.Name = "lbl_user";
            this.lbl_user.Size = new System.Drawing.Size(30, 13);
            this.lbl_user.TabIndex = 13;
            this.lbl_user.Text = "User";
            // 
            // lbl_password
            // 
            this.lbl_password.AutoSize = true;
            this.lbl_password.Location = new System.Drawing.Point(10, 202);
            this.lbl_password.Name = "lbl_password";
            this.lbl_password.Size = new System.Drawing.Size(56, 13);
            this.lbl_password.TabIndex = 14;
            this.lbl_password.Text = "Password";
            // 
            // lbl_folder
            // 
            this.lbl_folder.AutoSize = true;
            this.lbl_folder.Location = new System.Drawing.Point(10, 228);
            this.lbl_folder.Name = "lbl_folder";
            this.lbl_folder.Size = new System.Drawing.Size(50, 13);
            this.lbl_folder.TabIndex = 16;
            this.lbl_folder.Text = "FTP Path";
            // 
            // lbl_preview
            // 
            this.lbl_preview.AutoSize = true;
            this.lbl_preview.Location = new System.Drawing.Point(7, 252);
            this.lbl_preview.Name = "lbl_preview";
            this.lbl_preview.Size = new System.Drawing.Size(33, 13);
            this.lbl_preview.TabIndex = 17;
            this.lbl_preview.Text = "ftp://";
            // 
            // t_slider
            // 
            this.t_slider.Interval = 10;
            this.t_slider.Tick += new System.EventHandler(this.t_slider_Tick);
            // 
            // cb_clip
            // 
            this.cb_clip.AutoSize = true;
            this.cb_clip.Checked = true;
            this.cb_clip.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cb_clip.Location = new System.Drawing.Point(36, 352);
            this.cb_clip.Name = "cb_clip";
            this.cb_clip.Size = new System.Drawing.Size(146, 17);
            this.cb_clip.TabIndex = 9;
            this.cb_clip.Text = "Copy URLs to clipboard";
            this.cb_clip.UseVisualStyleBackColor = true;
            this.cb_clip.CheckedChanged += new System.EventHandler(this.cb_clip_CheckedChanged);
            // 
            // cb_shorten
            // 
            this.cb_shorten.AutoSize = true;
            this.cb_shorten.Checked = true;
            this.cb_shorten.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cb_shorten.Location = new System.Drawing.Point(36, 372);
            this.cb_shorten.Name = "cb_shorten";
            this.cb_shorten.Size = new System.Drawing.Size(81, 17);
            this.cb_shorten.TabIndex = 10;
            this.cb_shorten.Text = "Minify URL";
            this.cb_shorten.UseVisualStyleBackColor = true;
            this.cb_shorten.CheckedChanged += new System.EventHandler(this.cb_shorten_CheckedChanged);
            // 
            // th_shorten
            // 
            this.th_shorten.WorkerReportsProgress = true;
            this.th_shorten.DoWork += new System.ComponentModel.DoWorkEventHandler(this.th_shorten_DoWork);
            this.th_shorten.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.th_shorten_RunWorkerCompleted);
            // 
            // rdb_bitly
            // 
            this.rdb_bitly.AutoSize = true;
            this.rdb_bitly.Checked = true;
            this.rdb_bitly.Location = new System.Drawing.Point(61, 387);
            this.rdb_bitly.Name = "rdb_bitly";
            this.rdb_bitly.Size = new System.Drawing.Size(50, 17);
            this.rdb_bitly.TabIndex = 11;
            this.rdb_bitly.TabStop = true;
            this.rdb_bitly.Text = "bit.ly";
            this.rdb_bitly.UseVisualStyleBackColor = true;
            // 
            // rdb_googl
            // 
            this.rdb_googl.AutoSize = true;
            this.rdb_googl.Location = new System.Drawing.Point(61, 405);
            this.rdb_googl.Name = "rdb_googl";
            this.rdb_googl.Size = new System.Drawing.Size(59, 17);
            this.rdb_googl.TabIndex = 12;
            this.rdb_googl.Text = "goo.gl";
            this.rdb_googl.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(10, 277);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 13);
            this.label2.TabIndex = 23;
            this.label2.Text = "URL Path";
            // 
            // txt_url
            // 
            this.txt_url.Location = new System.Drawing.Point(66, 274);
            this.txt_url.Name = "txt_url";
            this.txt_url.Size = new System.Drawing.Size(188, 22);
            this.txt_url.TabIndex = 7;
            this.txt_url.TextChanged += new System.EventHandler(this.update_preview);
            // 
            // lbl_urlpreview
            // 
            this.lbl_urlpreview.AutoSize = true;
            this.lbl_urlpreview.Location = new System.Drawing.Point(7, 302);
            this.lbl_urlpreview.Name = "lbl_urlpreview";
            this.lbl_urlpreview.Size = new System.Drawing.Size(0, 13);
            this.lbl_urlpreview.TabIndex = 24;
            // 
            // cb_overwrite
            // 
            this.cb_overwrite.AutoSize = true;
            this.cb_overwrite.Checked = true;
            this.cb_overwrite.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cb_overwrite.Location = new System.Drawing.Point(36, 329);
            this.cb_overwrite.Name = "cb_overwrite";
            this.cb_overwrite.Size = new System.Drawing.Size(166, 17);
            this.cb_overwrite.TabIndex = 8;
            this.cb_overwrite.Text = "ask before overwriting files";
            this.cb_overwrite.UseVisualStyleBackColor = true;
            // 
            // frm_main
            // 
            this.ClientSize = new System.Drawing.Size(263, 437);
            this.Controls.Add(this.cb_overwrite);
            this.Controls.Add(this.lbl_urlpreview);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txt_url);
            this.Controls.Add(this.rdb_googl);
            this.Controls.Add(this.rdb_bitly);
            this.Controls.Add(this.cb_shorten);
            this.Controls.Add(this.cb_clip);
            this.Controls.Add(this.lbl_preview);
            this.Controls.Add(this.lbl_folder);
            this.Controls.Add(this.lbl_password);
            this.Controls.Add(this.lbl_user);
            this.Controls.Add(this.lbl_host);
            this.Controls.Add(this.txt_password);
            this.Controls.Add(this.cb_passive);
            this.Controls.Add(this.txt_port);
            this.Controls.Add(this.txt_user);
            this.Controls.Add(this.txt_folder);
            this.Controls.Add(this.txt_host);
            this.Controls.Add(this.pb_upload);
            this.Controls.Add(this.btn_save);
            this.Controls.Add(this.btn_settings);
            this.Controls.Add(this.p_dropzone);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "frm_main";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "FTPSchubser";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.frm_main_Load);
            this.p_dropzone.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Label label1;
        private Panel p_dropzone;
        private Button btn_settings;
        private BackgroundWorker th_upload;
        private Button btn_save;
        private ProgressBar pb_upload;
        private TextBox txt_host;
        private TextBox txt_folder;
        private TextBox txt_user;
        private TextBox txt_port;
        private CheckBox cb_passive;
        private MaskedTextBox txt_password;
        private Label lbl_host;
        private Label lbl_user;
        private Label lbl_password;
        private Label lbl_folder;
        private Label lbl_preview;
        private System.Windows.Forms.Timer t_slider;
        private CheckBox cb_clip;
        private CheckBox cb_shorten;
        private BackgroundWorker th_shorten;
        private RadioButton rdb_bitly;
        private RadioButton rdb_googl;
        private Label label2;
        private TextBox txt_url;
        private Label lbl_urlpreview;
        private CheckBox cb_overwrite;
    }
}

