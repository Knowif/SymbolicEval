using Android.App;
using Android.Widget;
using Android.OS;
using Android.Views;
using System;
using Symbolic;

namespace SymbolicAndroidUI
{
	[Activity(Label = "SymbolicAndroidUI", MainLauncher = true, Icon = "@mipmap/icon")]
	public class MainActivity : Activity
	{
		int count = 1;
		EditText text;
		TextView result;
		Button eval;

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			// Set our view from the "main" layout resource
			SetContentView(Resource.Layout.Main);

			text = FindViewById<EditText>(Resource.Id.editText1);
			result = FindViewById<TextView>(Resource.Id.textView1);
			eval = FindViewById<Button>(Resource.Id.button1);

			eval.Click += (sender, e) =>
			{
				try
				{
					Scanner scanner = new Scanner(text.Text);
					Parser parser = new Parser(scanner);
					Tree tree = parser.Parse();
					tree.Simplify();
					result.Text = tree.Print();
				} catch
				{
					result.Text = "ERROR";
				}
			};
		}
	}
}

