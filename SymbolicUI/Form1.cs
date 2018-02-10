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
				textBox2.Clear();
				Tree t = parser.Parse();
				var steps = t.Simplify();
				foreach (var step in steps)
				{
					textBox2.Text += "=" + step + "\n";
				}
			} catch (Exception)
			{
				textBox2.Text = "=ERROR";
			}
		}
	}
}
