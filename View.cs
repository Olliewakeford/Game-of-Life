using Gdk;
using Gtk;
using Cairo;
using Color = Cairo.Color;
using static MyWindow;
using Timeout = GLib.Timeout;

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
    GameView gameView;

    //dimensions of the grid
    public const int height = 50;
    public const int width = 75;
    public const int cellSize = 10;

    uint delay = 200; //delay between generations in milliseconds
    bool running = true;
    bool displayDefault = true; //whether to display the default starting state (gospers glider gun)
    uint timeoutId; //used to keep track of the timeout source ID

    // UI elements
    Box topVBox;
    MenuBar menuBar;
    MenuItem fileMenuItem;
    Menu fileMenu;

    public MyWindow() : base("Game of Life") {
        initializeUI();
        loadSpeedMenu();
        loadLifeSpanOptions();
        loadStartingStatesMenu();
    }

    void initializeUI(){
        Resize((width * cellSize) + 2 * GameView.margin, (height * cellSize) + 2 * GameView.margin + 50);
        topVBox = new Box(Orientation.Vertical, 2); // Create a vertical box 

        // Create the menu bar and menu items
        menuBar = new MenuBar();
        fileMenuItem = new MenuItem("Starting States");
        fileMenu = new Menu();
        
        topVBox.PackStart(menuBar, false, false, 0); // Add menu bar
        Add(topVBox); // Add vertical box to the window
    }


    void loadStartingStatesMenu(){
        // Get a list of files from the "StartingStates" directory
        DirectoryInfo dInfo = new DirectoryInfo("StartingStates");  
        FileInfo[] files = dInfo.GetFiles("*"); //all files in the directory

        // Iterate through each file in the directory and create a menu item for it
        foreach (FileInfo file in files) {
            MenuItem menuItem = new MenuItem(file.Name);

            // Attach an event handler for when the menu item is clicked
            menuItem.Activated += (sender, e) => {
                displayDefault = false;
                loadGameViewFromFile(file.Name);
                topVBox.ShowAll();
            };


            fileMenu.Append(menuItem); // Add the menu item to the menu
        }
        if (displayDefault) //if the user hasn't selected a starting state yet
            loadGameViewFromFile("gospers"); //load gospers gliding gun as default starting state

        fileMenuItem.Submenu = fileMenu; // Set the submenu of the menu item to the fileMenu
        menuBar.Append(fileMenuItem); // Add menu item to the menu bar
    }

    //Given a filename, load a game view with this starting state
    void loadGameViewFromFile(string filename){
        if (gameView != null) 
            topVBox.Remove(gameView); // Remove the previous game view

        if (filename == "random") //if the user wants a random starting state
            game = new GameOfLife(height, width);
        else //otherwise load the starting state from the files in the StartingStates directory
            game = new GameOfLife(height, width, loadGridState($"StartingStates/{filename}"));

        gameView = new GameView(game);
        topVBox.PackStart(gameView, true, true, 0);
        GLib.Source.Remove(timeoutId); // Remove the old timeout
        timeoutId = Timeout.Add((uint)delay, onTimeout); // Store the new timeout's ID
    }

    private void loadSpeedMenu() {
        Box hbox = new Box(Orientation.Horizontal, 0);
        RadioButton s = new RadioButton("slow");
        RadioButton m = new RadioButton(s, "medium");
        RadioButton f = new RadioButton(s, "fast");
        s.Clicked += onSlowClicked;
        m.Clicked += onMediumClicked;
        f.Clicked += onFastClicked;
        hbox.Add(new Label("Speed: ")); 
        hbox.Add(s);
        hbox.Add(m);
        hbox.Add(f);
        hbox.Margin = 5;
        topVBox.Add(hbox);
        Add(topVBox);
}

    //load the starting grid state from a file
    public static int[,] loadGridState(string filename){
        int[,] grid = new int[height, width];
        string[] lines = File.ReadAllLines(filename);
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

    void onSlowClicked(object? sender, EventArgs e) {
        delay = 200;
        GLib.Source.Remove(timeoutId); // Remove the old timeout
        timeoutId = Timeout.Add((uint)delay, onTimeout); // Store the new timeout's ID
    }

    void onMediumClicked(object? sender, EventArgs e) {
        delay = 75;
        GLib.Source.Remove(timeoutId); // Remove the old timeout
        timeoutId = Timeout.Add((uint)delay, onTimeout); // Store the new timeout's ID
    }

    void onFastClicked(object? sender, EventArgs e) {
        delay = 25;
        GLib.Source.Remove(timeoutId); // Remove the old timeout
        timeoutId = Timeout.Add((uint)delay, onTimeout); // Store the new timeout's ID
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

class Program {
    static void Main(){
        Application.Init();
        MyWindow w = new MyWindow();
        w.ShowAll();
        Application.Run();
    }
} 
