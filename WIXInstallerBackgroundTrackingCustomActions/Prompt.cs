using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace BackgroundTrackingCustomActions
{
    public static class Prompt
    {
        
        public static void ShowWaitDialog(string text, string caption)
        {
            PromptForm prompt = new PromptForm()
            {
                Width = 500,
                Height = 150,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = caption,
                StartPosition = FormStartPosition.CenterScreen
            };
            Label textLabel = new Label() { Left = 50, Top = 20, Text = text };
            textLabel.AutoSize = true;
            
            
            Button confirmation = new Button() { Text = "Ok", Left = 350, Width = 100, Top = 70, DialogResult = DialogResult.OK };
            confirmation.Click += (sender, e) => { prompt.Close(); };
            
            prompt.Controls.Add(confirmation);
            prompt.Controls.Add(textLabel);
            prompt.AcceptButton = confirmation;
            prompt.Controls.Add(textLabel);
            prompt.TopMost = true;
            prompt.ShowDialog();
            
        }
        

        private class PromptForm : Form
        {
           

            private void CloseMe()
            {
                this.Close();
            }

        }

    }
}