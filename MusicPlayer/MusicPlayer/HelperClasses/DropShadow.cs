using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace MusicPlayer
{
    class DropShadow : Form
    {
        bool FocusNextTime = true;
        Form parentForm;

        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        public DropShadow(Form parentForm, bool ManualFocus) : base()
        {
            parentForm.Closed += ParentForm_Closed; //Closes this when parent closes
            parentForm.Move += ParentForm_Move; //Follows movement of parent form
            this.GotFocus += DropShadow_GotFocus;
            this.Shown += DropShadow_Shown;
            if (!ManualFocus)
                parentForm.GotFocus += ParentForm_GotFocus;
            
            this.ShowInTaskbar = false;
            this.parentForm = parentForm;
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
        }

        private void DropShadow_Shown(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
                DropShadow_GotFocus(this, e);
            UpdateSizeLocation();
        }

        public void UpdateSizeLocation()
        {
            Size = new Size(parentForm.Size.Width, parentForm.Size.Height);
            Location = new Point(parentForm.Location.X, parentForm.Location.Y);
        }

        private void DropShadow_GotFocus(object sender, EventArgs e)
        {
            ShowWindow(Handle, 4);
            parentForm.BringToFront();
        }

        private void ParentForm_GotFocus(object sender, EventArgs e)
        {
            if (FocusNextTime)
            {
                this.Activate();
                if (parentForm != null)
                    parentForm.BringToFront();
            }
            FocusNextTime = !FocusNextTime;
        }

        private void ParentForm_Closed(object sender, EventArgs e)
        {
            this.Close();
        }
        
        private void ParentForm_Move(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
                DropShadow_GotFocus(this, e);
            UpdateSizeLocation();
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ClassStyle |= 0x00020000;
                return cp;
            }
        }
    }
}
