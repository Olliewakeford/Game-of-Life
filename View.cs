using Gdk;
using Gtk;


class MyWindow : Gtk.Window {
    public MyWindow() : base("Game of Life") {

    }

    protected override bool OnDeleteEvent(Event e) {
        Application.Quit();
        return true;
    }
}

class Hello {
    static void Main1() {
        Application.Init();
        MyWindow w = new MyWindow();
        w.ShowAll();
        Application.Run();
    }
}