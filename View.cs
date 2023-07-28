using Gdk;
using Gtk;
using Cairo;
using Color = Cairo.Color;

class GameView : DrawingArea {
    Color black = new Color(0, 0, 0);
    GameOfLife game;
    int height;
    int width;

    public GameView(GameOfLife game, int height, int width) {
        this.game = game;
       this.height = height;
        this.width = width;
    }

    // draw a rectangle at (x,y) with width and height of 5
    protected void drawDot(Context c, double x, double y) { 
        c.LineWidth = 5;
        c.Rectangle(x, y, 5, 5);
        c.Fill();
    }


    protected override bool OnDrawn (Context c){ 
        c.SetSourceColor(black);
        int[,] currGrid = game.gridProperty;
        double x = 0, y = 0;

        for (int i = 0; i < height; i++){
            for (int j = 0; j < width; j++){
                if (currGrid[i,j] == 1)
                    drawDot(c, x, y);
                x += 5; // increase x by cell size
            }
            y += 5; // increase y by cell size
            x = 0; // reset x after each row
        }
        return true;
    }
}

class MyWindow : Gtk.Window {
    int height = 50;
    int width = 75;

    public MyWindow() : base("Game of Life") {
        Resize(800, 600);
        GameOfLife game = new GameOfLife(height, width);
        Add(new GameView(game, height, width));
    }

    public int Width => width;
    public int Height => height;

    protected override bool OnDeleteEvent(Event e) {
        Application.Quit();
        return true;
    }
}

class Top {
    static void Main() {
        Application.Init();
        MyWindow w = new MyWindow();
        w.ShowAll();
        Application.Run();
    }
}