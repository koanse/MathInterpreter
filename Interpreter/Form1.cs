using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Globalization;

namespace Interpreter
{
    public partial class Form1 : Form
    {
        Interpreter interpreter;
        public Form1()
        {
            InitializeComponent();
            interpreter = new Interpreter();
        }

        private void toolStripTextBox1_TextChanged(object sender, EventArgs e)
        {
            try
            {
                string s = toolStripTextBox1.Text;
                if (s == "")
                {
                    toolStripTextBox3.Text = "";
                    return;
                }
                interpreter.Interpret(s);
                toolStripTextBox3.Text = interpreter.Result().ToString();
            }
            catch (Exception ex)
            {
                toolStripTextBox3.Text = ex.Message;
            }
        }
        private void toolStripComboBox1_TextUpdate(object sender, EventArgs e)
        {
            string varName = toolStripComboBox1.Text;
            try
            {
                toolStripTextBox2.Text = interpreter.GetVarValue(varName).ToString();
            }
            catch { }
        }
        private void toolStripComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string varName = toolStripComboBox1.Text;
            try
            {
                toolStripTextBox2.Text = interpreter.GetVarValue(varName).ToString();
            }
            catch { }
        }
        private void toolStripTextBox2_TextChanged(object sender, EventArgs e)
        {
            string varName = toolStripComboBox1.Text;
            string varValue = toolStripTextBox2.Text;
            float value = 0.0f;
            if(varName == "") return;
            if(toolStripComboBox1.Items.Contains(varName) == false)
            {
                interpreter.AddVariable(varName);
                toolStripComboBox1.Items.Add(varName);
            }
            CultureInfo MyCultureInfo = new CultureInfo("en-US");
            MyCultureInfo.NumberFormat.NumberDecimalSeparator = ".";
            try
            {
                value = float.Parse(varValue, NumberStyles.AllowDecimalPoint, MyCultureInfo);
            }
            catch { }
            if (toolStripComboBox1.Items.Contains(varName))
                interpreter.SetVarValue(varName, value);
            toolStripTextBox3.Text = value.ToString();
            toolStripTextBox1_TextChanged(null, null);
            interpreter.SetVarValue(varName, value);
        }
    }
}