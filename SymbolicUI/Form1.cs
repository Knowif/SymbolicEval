using System;
using System.Windows.Forms;
using Symbolic;

namespace SymbolicUI
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
		}

		private void button1_Click(object sender, EventArgs e)
		{
			Scanner scan = new Scanner(textBox1.Text);
			Parser parser = new Parser(scan);
			try
			{
				Tree t = parser.Parse();
				t.Simplify();
				textBox2.Text = t.Print();
			} catch (Exception)
			{
				textBox2.Text = "ERROR";
			}
		}
	}
}
