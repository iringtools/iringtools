using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace DbDictionaryEditor
{
    public partial class NewDbDictionary : ChildWindow
    {
        public NewDbDictionary()
        {
            InitializeComponent();
        }

        private void btnAccept_Click(object sender, RoutedEventArgs e)
        {
            tbMessages.Text = null;
               
            if (tbProject.Text.Length == 0)
            {
                tbMessages.Text = "Project cannot be null";
                tbProject.Focus();
                this.DialogResult = false;
            } else if (tbApp.Text.Length == 0)
            {
                tbMessages.Text = "Application cannot be null";
                tbApp.Focus();
                this.DialogResult = false;
            } else if (tbNewDataSource.Text.Length == 0)
            {
                tbMessages.Text = "Data Source cannot be null";
                tbNewDataSource.Focus();
                this.DialogResult = false;
            } else if (tbNewDatabase.Text.Length == 0)
            {
                tbMessages.Text = "Database cannot be null";
                tbNewDatabase.Focus();
                this.DialogResult = false;
            } else if (tbUserID.Text.Length == 0)
            {
                tbMessages.Text = "User ID cannot be null";
                tbUserID.Focus();
                this.DialogResult = false;
            } else if (tbPassword.Text.Length == 0)
            {
                tbMessages.Text = "Password cannot be null";
                tbPassword.Focus();
                this.DialogResult = false;
            } else
                this.DialogResult = true;
            
        }

        private void btnCancl_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

    }
}
