using Gdk;
using Gtk;
using Cairo;
using Color = Cairo.Color;
using static MyWindow;
using Timeout = GLib.Timeout;
using System.IO;

class GameView : DrawingArea {
    Color black = new Color(0, 0, 0);
    GameOfLife game;
    bool running = true; 
    public const int margin = 10; // Margin around the grid

    public GameView(GameOfLife game){
        this.game = game;
    }

    // draw a square at (x,y) with width and height
    protected void drawDot(Context c, double x, double y){ 
        c.Rectangle(x, y, cellSize, cellSize);
        c.Fill();
    }

    //draw the entire grid, representing alive cells as a black square
    void drawGrid(Context c, int[,] currGrid){
        double x = margin, y = margin;

        for (int i = 0; i < height; i++){
            for (int j = 0; j < width; j++){
                if (currGrid[i,j] == 1)
                    drawDot(c, x, y);
                x +=  cellSize; // increase x by cell size
            }
            y += cellSize; // increase y by cell size
            x = margin; // reset x after each row
        }
    }

    protected override bool OnDrawn (Context c){ 
        c.SetSourceColor(black);
        int[,] currGrid = game.gridProperty;
        if (running)
            drawGrid(c, currGrid);
        return true;
    }

    
}

class MyWindow : Gtk.Window {
    GameOfLife game;
    public const int height = 50;
    public const int width = 75;
    public const int cellSize = 5;
    bool running = true;
    VBox vbox;
    GameView gameView;
    MenuBar menuBar;
    MenuItem fileMenuItem;
    Menu fileMenu;

    public MyWindow() : base("Game of Life"){
        initializeUI();
        loadStartingStatesMenu();
    }

    private void initializeUI(){
        Resize((width * cellSize) + 2 * GameView.margin, (height * cellSize) + 2 * GameView.margin);
        vbox = new VBox(false, 2); // Create a vertical box to hold the menu bar

        // Create the menu bar and menu items
        menuBar = new MenuBar();
        fileMenuItem = new MenuItem("Starting States");
        fileMenu = new Menu();
        
        vbox.PackStart(menuBar, false, false, 0); // Add menu bar to vertical box
        Add(vbox); // Add vertical box to the window
    }

    private void loadStartingStatesMenu(){
        // Get a list of files from the "StartingStates" directory
        DirectoryInfo dInfo = new DirectoryInfo("StartingStates");
        FileInfo[] files = dInfo.GetFiles("*");

        // Iterate through each file in the directory and create a menu item for it
        foreach (FileInfo file in files) {
            MenuItem menuItem = new MenuItem(file.Name);

            // Attach an event handler for when the menu item is clicked
            menuItem.Activated += (sender, e) => {
                loadGameViewFromFile(file.Name);
                vbox.ShowAll();
            };

            fileMenu.Append(menuItem); // Add the menu item to the menu
        }

        fileMenuItem.Submenu = fileMenu; // Set the submenu of the menu item to the fileMenu
        menuBar.Append(fileMenuItem); // Add menu item to the menu bar
    }

    //Given a filename, load a game view with this starting state
    private void loadGameViewFromFile(string filename){
        if (gameView != null) 
            vbox.Remove(gameView); // Remove the previous game view

        game = new GameOfLife(height, width, loadGridState($"StartingStates/{filename}"));
        gameView = new GameView(game);

        vbox.PackStart(gameView, true, true, 0);
        Timeout.Add(50, onTimeout); 
    }

    //load the starting grid state from a file
    public static int[,] loadGridState(string filename){
        int[,] grid = new int[height, width];
        string[] lines = System.IO.File.ReadAllLines(filename);
        for (int i = 0; i < lines.Length; i++){
            for (int j = 0; j < lines[i].Length; j++){
                if (lines[i][j] == '1')
                    grid[i,j] = 1;
                else
                    grid[i,j] = 0;
            }
        }
        return grid;
    }

    //called to update the grid each generation
    bool onTimeout(){
        if (running)
            game.nextGen();
        QueueDraw();
        return true;
    }

    //allow the user to pause the game using the space bar
    protected override bool OnKeyPressEvent(EventKey evnt){
        if (evnt.Key == Gdk.Key.space)
            running = !running;
        return true;
    }

    //close the window
    protected override bool OnDeleteEvent(Event e){
        Application.Quit();
        return true;
    }
}

class Top {
    static void Main(){
        Application.Init();
        MyWindow w = new MyWindow();
        w.ShowAll();
        Application.Run();
    }
} 
