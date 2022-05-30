namespace MomoPush
{
    partial class MomoPush
    {
        /// <summary>
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 디자이너에서 생성한 코드

        /// <summary>
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.titleBox = new System.Windows.Forms.TextBox();
            this.msgBox = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.deviceIdBox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.button2 = new System.Windows.Forms.Button();
            this.curTime = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.remainTime = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.listLog = new System.Windows.Forms.ListView();
            this.LogTime = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.Log = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.S_Id = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.listSchedule = new System.Windows.Forms.ListView();
            this.Id = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.GroupId = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.Title = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.Time = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.CurrentTimer = new System.Windows.Forms.Timer(this.components);
            this.SetTimer = new System.Windows.Forms.Timer(this.components);
            this.RemainTimer = new System.Windows.Forms.Timer(this.components);
            this.label9 = new System.Windows.Forms.Label();
            this.ipBox = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.portBox = new System.Windows.Forms.TextBox();
            this.btnStart = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 74);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(58, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "Message";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 109);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 12);
            this.label2.TabIndex = 1;
            this.label2.Text = "DeviceId";
            // 
            // titleBox
            // 
            this.titleBox.Location = new System.Drawing.Point(87, 36);
            this.titleBox.Name = "titleBox";
            this.titleBox.Size = new System.Drawing.Size(385, 21);
            this.titleBox.TabIndex = 2;
            // 
            // msgBox
            // 
            this.msgBox.Location = new System.Drawing.Point(87, 71);
            this.msgBox.Name = "msgBox";
            this.msgBox.Size = new System.Drawing.Size(385, 21);
            this.msgBox.TabIndex = 3;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(492, 36);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(101, 56);
            this.button1.TabIndex = 4;
            this.button1.Text = "Send Push";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.sendPush_Click);
            // 
            // deviceIdBox
            // 
            this.deviceIdBox.Location = new System.Drawing.Point(87, 106);
            this.deviceIdBox.Name = "deviceIdBox";
            this.deviceIdBox.Size = new System.Drawing.Size(385, 21);
            this.deviceIdBox.TabIndex = 6;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(13, 39);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(29, 12);
            this.label3.TabIndex = 7;
            this.label3.Text = "Title";
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(452, 20);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(141, 45);
            this.button2.TabIndex = 8;
            this.button2.Text = "Reload Schedule";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.loadSchedule_Click);
            // 
            // curTime
            // 
            this.curTime.AutoSize = true;
            this.curTime.Font = new System.Drawing.Font("굴림", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.curTime.ForeColor = System.Drawing.Color.Red;
            this.curTime.Location = new System.Drawing.Point(103, 29);
            this.curTime.Name = "curTime";
            this.curTime.Size = new System.Drawing.Size(11, 12);
            this.curTime.TabIndex = 9;
            this.curTime.Text = "-";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.groupBox);
            this.groupBox1.Controls.Add(this.label8);
            this.groupBox1.Controls.Add(this.titleBox);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.msgBox);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.button1);
            this.groupBox1.Controls.Add(this.deviceIdBox);
            this.groupBox1.Location = new System.Drawing.Point(13, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(599, 146);
            this.groupBox1.TabIndex = 11;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Push";
            // 
            // groupBox
            // 
            this.groupBox.Location = new System.Drawing.Point(545, 106);
            this.groupBox.Name = "groupBox";
            this.groupBox.Size = new System.Drawing.Size(48, 21);
            this.groupBox.TabIndex = 9;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(490, 109);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(49, 12);
            this.label8.TabIndex = 8;
            this.label8.Text = "GroupId";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.remainTime);
            this.groupBox2.Controls.Add(this.label7);
            this.groupBox2.Controls.Add(this.listLog);
            this.groupBox2.Controls.Add(this.listSchedule);
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.curTime);
            this.groupBox2.Controls.Add(this.button2);
            this.groupBox2.Location = new System.Drawing.Point(13, 173);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(599, 455);
            this.groupBox2.TabIndex = 12;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Schedule";
            // 
            // remainTime
            // 
            this.remainTime.AutoSize = true;
            this.remainTime.ForeColor = System.Drawing.Color.ForestGreen;
            this.remainTime.Location = new System.Drawing.Point(103, 53);
            this.remainTime.Name = "remainTime";
            this.remainTime.Size = new System.Drawing.Size(11, 12);
            this.remainTime.TabIndex = 18;
            this.remainTime.Text = "-";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(6, 53);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(93, 12);
            this.label7.TabIndex = 17;
            this.label7.Text = "[Remain Time]";
            // 
            // listLog
            // 
            this.listLog.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.listLog.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.LogTime,
            this.Log,
            this.S_Id});
            this.listLog.FullRowSelect = true;
            this.listLog.HideSelection = false;
            this.listLog.Location = new System.Drawing.Point(6, 281);
            this.listLog.Name = "listLog";
            this.listLog.Size = new System.Drawing.Size(587, 168);
            this.listLog.TabIndex = 16;
            this.listLog.UseCompatibleStateImageBehavior = false;
            this.listLog.View = System.Windows.Forms.View.Details;
            // 
            // LogTime
            // 
            this.LogTime.Text = "Time";
            this.LogTime.Width = 180;
            // 
            // Log
            // 
            this.Log.Text = "Log";
            this.Log.Width = 350;
            // 
            // S_Id
            // 
            this.S_Id.Text = "S_Id";
            this.S_Id.Width = 40;
            // 
            // listSchedule
            // 
            this.listSchedule.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.listSchedule.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.Id,
            this.GroupId,
            this.Title,
            this.Time});
            this.listSchedule.FullRowSelect = true;
            this.listSchedule.HideSelection = false;
            this.listSchedule.Location = new System.Drawing.Point(6, 94);
            this.listSchedule.Name = "listSchedule";
            this.listSchedule.Size = new System.Drawing.Size(587, 159);
            this.listSchedule.TabIndex = 15;
            this.listSchedule.UseCompatibleStateImageBehavior = false;
            this.listSchedule.View = System.Windows.Forms.View.Details;
            // 
            // Id
            // 
            this.Id.Text = "Id";
            this.Id.Width = 40;
            // 
            // GroupId
            // 
            this.GroupId.Text = "Group Id";
            this.GroupId.Width = 65;
            // 
            // Title
            // 
            this.Title.Text = "Title";
            this.Title.Width = 230;
            // 
            // Time
            // 
            this.Time.Text = "Time";
            this.Time.Width = 230;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(7, 266);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(59, 12);
            this.label6.TabIndex = 14;
            this.label6.Text = "Push Log";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(7, 80);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(78, 12);
            this.label5.TabIndex = 13;
            this.label5.Text = "Schedult List";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 29);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(91, 12);
            this.label4.TabIndex = 11;
            this.label4.Text = "[Current Time]";
            // 
            // CurrentTimer
            // 
            this.CurrentTimer.Enabled = true;
            this.CurrentTimer.Interval = 1000;
            this.CurrentTimer.Tick += new System.EventHandler(this.timer_Tick);
            // 
            // SetTimer
            // 
            this.SetTimer.Interval = 1000;
            this.SetTimer.Tick += new System.EventHandler(this.SetTimer_Tick);
            // 
            // RemainTimer
            // 
            this.RemainTimer.Enabled = true;
            this.RemainTimer.Interval = 1000;
            this.RemainTimer.Tick += new System.EventHandler(this.RemainTimer_Tick);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(17, 645);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(16, 12);
            this.label9.TabIndex = 10;
            this.label9.Text = "IP";
            // 
            // ipBox
            // 
            this.ipBox.Location = new System.Drawing.Point(39, 640);
            this.ipBox.Name = "ipBox";
            this.ipBox.Size = new System.Drawing.Size(240, 21);
            this.ipBox.TabIndex = 10;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(300, 645);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(27, 12);
            this.label10.TabIndex = 13;
            this.label10.Text = "Port";
            // 
            // portBox
            // 
            this.portBox.Location = new System.Drawing.Point(333, 640);
            this.portBox.Name = "portBox";
            this.portBox.Size = new System.Drawing.Size(108, 21);
            this.portBox.TabIndex = 14;
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(465, 634);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(141, 30);
            this.btnStart.TabIndex = 10;
            this.btnStart.Text = "Start Server";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.ServerStart_Click);
            // 
            // MomoPush
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(624, 676);
            this.Controls.Add(this.btnStart);
            this.Controls.Add(this.portBox);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.ipBox);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Font = new System.Drawing.Font("굴림", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.Name = "MomoPush";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Momo Push Server";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox titleBox;
        private System.Windows.Forms.TextBox msgBox;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox deviceIdBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Label curTime;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Timer CurrentTimer;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ListView listSchedule;
        private System.Windows.Forms.ColumnHeader Id;
        private System.Windows.Forms.ColumnHeader GroupId;
        private System.Windows.Forms.ColumnHeader Title;
        private System.Windows.Forms.ColumnHeader Time;
        private System.Windows.Forms.ListView listLog;
        private System.Windows.Forms.ColumnHeader S_Id;
        private System.Windows.Forms.ColumnHeader LogTime;
        private System.Windows.Forms.ColumnHeader Log;
        private System.Windows.Forms.Label remainTime;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Timer SetTimer;
        private System.Windows.Forms.Timer RemainTimer;
        private System.Windows.Forms.TextBox groupBox;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox ipBox;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox portBox;
        private System.Windows.Forms.Button btnStart;
    }
}

