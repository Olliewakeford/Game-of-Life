using Gdk;
using Gtk;
using Cairo;
using Color = Cairo.Color;
using static MyWindow;
using Timeout = GLib.Timeout;

class GameView : DrawingArea {
    Color black = new Color(0, 0, 0);
    GameOfLife game;
    bool running = false;

    public GameView(GameOfLife game) {
        this.game = game;
    }

    // draw a rectangle at (x,y) with width and height of 5
    protected void drawDot(Context c, double x, double y) { 
        c.Rectangle(x, y, cellSize, cellSize);
        c.Fill();
    }

    void drawGrid(Context c, int[,] currGrid){
        double x = 0, y = 0;

        for (int i = 0; i < height; i++){
            for (int j = 0; j < width; j++){
                if (currGrid[i,j] == 1)
                    drawDot(c, x, y);
                x +=  cellSize; // increase x by cell size
            }
            y += cellSize; // increase y by cell size
            x = 0; // reset x after each row
        }
    }

    void nextIteration() {
        game.nextGen();
        QueueDraw();
    }

    protected override bool OnDrawn (Context c){ 
        c.SetSourceColor(black);
        int[,] currGrid = game.gridProperty;
        drawGrid(c, currGrid);
        return true;
    }
}

class MyWindow : Gtk.Window {
    GameOfLife game = new GameOfLife(height, width);
    public const int height = 50;
    public const int width = 75;
    public const int cellSize = 5;

    public MyWindow() : base("Game of Life") {
        Resize(width * cellSize, height * cellSize);
        Add(new GameView(game));
        Timeout.Add(500, onTimeout); 
    }

    bool onTimeout() {
        game.nextGen();
        QueueDraw();
        return true;
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