using Android.App;
using Android.OS;
using Android.Widget;

namespace SnakeAndroid;

[Activity(Label = "@string/app_name", MainLauncher = true)]
public class MainActivity : Activity
{
    private SnakeView? _snakeView;

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        var root = new LinearLayout(this)
        {
            Orientation = Orientation.Vertical
        };

        _snakeView = new SnakeView(this);
        var gameParams = new LinearLayout.LayoutParams(
            LinearLayout.LayoutParams.MatchParent,
            0,
            1f
        );
        root.AddView(_snakeView, gameParams);

        var controls = BuildDirectionControls();
        root.AddView(controls);

        SetContentView(root);
    }

    protected override void OnResume()
    {
        base.OnResume();
        _snakeView?.StartLoop();
    }

    protected override void OnPause()
    {
        base.OnPause();
        _snakeView?.StopLoop();
    }

    private LinearLayout BuildDirectionControls()
    {
        var pad = new LinearLayout(this)
        {
            Orientation = Orientation.Vertical
        };
        pad.SetPadding(24, 12, 24, 24);

        var topRow = new LinearLayout(this) { Orientation = Orientation.Horizontal };
        topRow.SetGravity(Android.Views.GravityFlags.CenterHorizontal);
        topRow.AddView(CreateDirButton("Atas", Direction.Up));


        var bottomRow = new LinearLayout(this) { Orientation = Orientation.Horizontal };
        bottomRow.SetGravity(Android.Views.GravityFlags.CenterHorizontal);
        bottomRow.AddView(CreateDirButton("Kiri", Direction.Left));
        bottomRow.AddView(CreateDirButton("Bawah", Direction.Down));
        bottomRow.AddView(CreateDirButton("Kanan", Direction.Right));

        pad.AddView(topRow);
        pad.AddView(bottomRow);
        return pad;
    }

    private Button CreateDirButton(string label, Direction dir)
    {
        var btn = new Button(this) { Text = label };
        btn.Click += (_, _) => _snakeView?.ChangeDirection(dir);
        return btn;
    }
}
