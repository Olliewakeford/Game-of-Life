using Gdk;
using Gtk;
using Cairo;
using Color = Cairo.Color;
using static MyWindow;
using Timeout = GLib.Timeout;

//the view of the game, responsible for drawing the grid
class GameView : DrawingArea {
    Color black = new Color(0, 0, 0);
    GameOfLife game;
    bool running = true; 
    public const int margin = 10; // Margin around the grid

    public GameView(GameOfLife game){
        this.game = game;
    }

    // draw a square at (x,y) with the cellSize width and height
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

//class to handle the whole window representing the game and its extras
class MyWindow : Gtk.Window {
    GameOfLife game;
    GameView gameView;

    //dimensions of the grid
    public const int height = 50;
    public const int width = 75;
    public const int cellSize = 10;

    //game settings
    int delay = 200; //delay between generations in milliseconds
    bool unPaused = true; //true if the game is unpaused
    bool displayDefault = true; //whether to display the default starting state (gospers glider gun)
    uint timeoutTracker; //used to keep track of the timeout source ID
    int lifeSpan = -1; //number of generations to run the game for. -1 means run forever
    int slowSpeed = 200; //delay between generations when running slowly
    int mediumSpeed = 75; //delay between generations when running at medium speed
    int fastSpeed = 25; //delay between generations when running fast
    int spinButtonIncrement = 10; //increment for the spin button

    // UI elements
    Box vBox;
    MenuBar menuBar;
    MenuItem fileMenuItem;
    Menu fileMenu;
    SpinButton lifeSpanSpinner; //spin button for selecting the number of generations to run the game for
    CheckButton eternalLife; //checkbox for selecting whether to run the game forever

    public MyWindow() : base("Game of Life") {
        initializeUI();
        loadSpeedMenu();
        loadLifeSpanOptions();
        loadStartingStatesMenu();
    }

    //set up the basics of the window with a menu
    void initializeUI(){
        Resize((width * cellSize) + 2 * GameView.margin, (height * cellSize) + 2 * GameView.margin + 200);
        vBox = new Box(Orientation.Vertical, 2); // Create a vertical box 

        // Create the menu bar and menu items
        menuBar = new MenuBar();
        fileMenuItem = new MenuItem("Starting States");
        fileMenu = new Menu();
        
        vBox.PackStart(menuBar, false, false, 0); // Add menu bar
        Add(vBox); // Add vertical box to the window
    }

    //create a menu for the user to select starting states from
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
                vBox.ShowAll();
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
            vBox.Remove(gameView); // Remove the previous game view

        if (filename == "random") //if the user wants a random starting state
            game = new GameOfLife(height, width);
        else //otherwise load the starting state from the files in the StartingStates directory
            game = new GameOfLife(height, width, loadGridState($"StartingStates/{filename}"));

        gameView = new GameView(game);
        vBox.PackStart(gameView, true, true, 0);
        GLib.Source.Remove(timeoutTracker); // Remove the old timeout
        timeoutTracker = Timeout.Add((uint)delay, onTimeout); // Store the new timeout's ID
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

    //radio buttons to select speed of the Game of Life
    private void loadSpeedMenu(){
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
        vBox.Add(hbox);
        Add(vBox);
}

    //event handlers for the speed radio buttons
    void onSlowClicked(object? sender, EventArgs e) {
        delay = slowSpeed;
        GLib.Source.Remove(timeoutTracker); // Remove the old timeout
        timeoutTracker = Timeout.Add((uint)delay, onTimeout); // Store the new timeout's ID
    }

    void onMediumClicked(object? sender, EventArgs e) {
        delay = mediumSpeed;
        GLib.Source.Remove(timeoutTracker); // Remove the old timeout
        timeoutTracker = Timeout.Add((uint)delay, onTimeout); // Store the new timeout's ID
    }

    void onFastClicked(object? sender, EventArgs e) {
        delay = fastSpeed;
        GLib.Source.Remove(timeoutTracker); // Remove the old timeout
        timeoutTracker = Timeout.Add((uint)delay, onTimeout); // Store the new timeout's ID
    }

    void loadLifeSpanOptions(){
        Box hbox = new Box(Orientation.Horizontal, 0); //box to hold life span options
        lifeSpanSpinner = new SpinButton(-1, 1000, spinButtonIncrement); //spin button for selecting the number of generations to play game
        lifeSpanSpinner.Value = -1 ; //-1 means run forever and is the default setting
        lifeSpanSpinner.ValueChanged += onLifeSpanChanged;
        hbox.Add(new Label("Life Span: "));
        hbox.Add(lifeSpanSpinner);
        eternalLife = new CheckButton("Eternal Life"); //checkbox for selecting whether to run the game forever
        eternalLife.Active = true;
        eternalLife.Clicked += onEternalLifeClicked;
        hbox.Add(eternalLife);
        hbox.Margin = 5;
        vBox.Add(hbox);
        Add(vBox);
    }
    
    //event handlers for the life span options
    void onLifeSpanChanged(object? sender, EventArgs e){
        if (lifeSpan > 0)
            eternalLife.Active = false;
        lifeSpan = (int)lifeSpanSpinner.Value;
    }

    void onEternalLifeClicked(object? sender, EventArgs e){
        if (eternalLife.Active){
            lifeSpan = -1;
            lifeSpanSpinner.Value = -1;
        }
        else {
            lifeSpan = 0;
            lifeSpanSpinner.Value = 0;
        }
    }

    //called to update the grid every timeout interval
    bool onTimeout(){
        if (unPaused){ //game is not paused
            if (lifeSpan > 0){ //if the user has selected a number of generations to play
                lifeSpan--;
                game.nextGen();
                lifeSpanSpinner.Value = lifeSpan;
            }
            else if (lifeSpan == -1) //if the user has selected to play forever
                game.nextGen();
        }
        QueueDraw();
        return true;
    }


    //allow the user to pause the game using the space bar
    protected override bool OnKeyPressEvent(EventKey evnt){
        if (evnt.Key == Gdk.Key.space)
            unPaused = !unPaused;
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
