namespace LocalMessagesApp
{
    partial class Form1
    {
        /// <summary>
        /// Variable del diseñador necesaria.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Limpiar los recursos que se estén usando.
        /// </summary>
        /// <param name="disposing">true si los recursos administrados se deben desechar; false en caso contrario.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código generado por el Diseñador de Windows Forms

        /// <summary>
        /// Método necesario para admitir el Diseñador. No se puede modificar
        /// el contenido de este método con el editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnConection = new System.Windows.Forms.Button();
            this.tbxUserName = new System.Windows.Forms.TextBox();
            this.tbxReceiver = new System.Windows.Forms.TextBox();
            this.tbxMessage = new System.Windows.Forms.TextBox();
            this.BtnSend = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.lbxUssers = new System.Windows.Forms.ListBox();
            this.lbxChat = new System.Windows.Forms.ListBox();
            this.SuspendLayout();
            // 
            // btnConection
            // 
            this.btnConection.Location = new System.Drawing.Point(25, 478);
            this.btnConection.Name = "btnConection";
            this.btnConection.Size = new System.Drawing.Size(96, 32);
            this.btnConection.TabIndex = 0;
            this.btnConection.Text = "Connect";
            this.btnConection.UseVisualStyleBackColor = true;
            this.btnConection.Click += new System.EventHandler(this.btnConection_Click);
            // 
            // tbxUserName
            // 
            this.tbxUserName.Location = new System.Drawing.Point(25, 434);
            this.tbxUserName.Name = "tbxUserName";
            this.tbxUserName.Size = new System.Drawing.Size(100, 22);
            this.tbxUserName.TabIndex = 1;
            // 
            // tbxReceiver
            // 
            this.tbxReceiver.Location = new System.Drawing.Point(192, 497);
            this.tbxReceiver.Name = "tbxReceiver";
            this.tbxReceiver.Size = new System.Drawing.Size(100, 22);
            this.tbxReceiver.TabIndex = 2;
            // 
            // tbxMessage
            // 
            this.tbxMessage.Location = new System.Drawing.Point(298, 497);
            this.tbxMessage.Name = "tbxMessage";
            this.tbxMessage.Size = new System.Drawing.Size(498, 22);
            this.tbxMessage.TabIndex = 3;
            // 
            // BtnSend
            // 
            this.BtnSend.Location = new System.Drawing.Point(802, 496);
            this.BtnSend.Name = "BtnSend";
            this.BtnSend.Size = new System.Drawing.Size(120, 23);
            this.BtnSend.TabIndex = 4;
            this.BtnSend.Text = "Send";
            this.BtnSend.UseVisualStyleBackColor = true;
            this.BtnSend.Click += new System.EventHandler(this.BtnSend_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(189, 478);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 16);
            this.label1.TabIndex = 5;
            this.label1.Text = "Receiver:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(295, 478);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(64, 16);
            this.label2.TabIndex = 6;
            this.label2.Text = "Message";
            // 
            // lbxUssers
            // 
            this.lbxUssers.FormattingEnabled = true;
            this.lbxUssers.ItemHeight = 16;
            this.lbxUssers.Location = new System.Drawing.Point(802, 116);
            this.lbxUssers.Name = "lbxUssers";
            this.lbxUssers.Size = new System.Drawing.Size(120, 340);
            this.lbxUssers.TabIndex = 7;
            // 
            // lbxChat
            // 
            this.lbxChat.FormattingEnabled = true;
            this.lbxChat.ItemHeight = 16;
            this.lbxChat.Location = new System.Drawing.Point(192, 116);
            this.lbxChat.Name = "lbxChat";
            this.lbxChat.Size = new System.Drawing.Size(604, 340);
            this.lbxChat.TabIndex = 8;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Highlight;
            this.ClientSize = new System.Drawing.Size(945, 531);
            this.Controls.Add(this.lbxChat);
            this.Controls.Add(this.lbxUssers);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.BtnSend);
            this.Controls.Add(this.tbxMessage);
            this.Controls.Add(this.tbxReceiver);
            this.Controls.Add(this.tbxUserName);
            this.Controls.Add(this.btnConection);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnConection;
        private System.Windows.Forms.TextBox tbxUserName;
        private System.Windows.Forms.TextBox tbxReceiver;
        private System.Windows.Forms.TextBox tbxMessage;
        private System.Windows.Forms.Button BtnSend;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ListBox lbxUssers;
        private System.Windows.Forms.ListBox lbxChat;
    }
}

