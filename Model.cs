using System;
using System.Collections.Generic;
using static System.Console;

static class Grid {
    //return a grid of 0s
    public static int[,] clearGrid(int height, int width){
        int[,] grid = new int[height, width];

        for(int i = 0; i < height; i++){
            for(int j = 0; j < width; j++)
                grid[i,j] = 0;
        }
        
        return grid;
    }
    
    //return a grid of random 0s and 1s
    public static int[,] randomGrid(int height, int width){
        Random rand = new Random();
        int[,] grid = new int[height, width];

        for(int i = 0; i < height; i++){
            for(int j = 0; j < width; j++){
                grid[i, j] = rand.Next(0,2); //look into adjusting probability
            }
        }
        
        return grid;
    }
}

class GameOfLife {
    private int height;
    private int width;

    private int[,] grid;

    //initialise a grid with random starting state
    public GameOfLife(int height, int width){
        this.height = height;
        this.width = width;
        this.grid = Grid.randomGrid(height, width);
    }

    //initialise a grid with a given starting state
    public GameOfLife(int height, int width, int[,] grid){
        //initialise a board based on input
        this.height = height;
        this.width = width;
        this.grid = grid;
    }

    public int[,] gridProperty {
        get => grid;
        set => grid = value;
    }


    //return the number of live neighbours of a given cell
    public int countLiveNeighbours(int row, int column){
        int liveNeighboursCount = 0;

        // Offsets for the 8 potential neighbouring cells.
        int[] neighbourRowOffsets = {-1, -1, -1, 0, 0, 1, 1, 1};
        int[] neighbourColumnOffsets = {-1, 0, 1, -1, 1, -1, 0, 1};

        for (int i = 0; i < neighbourRowOffsets.Length; i++){
            // Calculate the row and column indices of the neighbouring cell
            int neighbourRow = row + neighbourRowOffsets[i]; //-1 if out of bounds
            int neighbourColumn = column + neighbourColumnOffsets[i]; //-1 if out of bounds

            // Check if the calculated neighbour cell is within the grid boundaries.
            if (neighbourRow >= 0 && neighbourRow < height){
                if (neighbourColumn >= 0 && neighbourColumn < width) {
                    // If the neighbour cell is alive (has the value 1), increment the count.
                    if (grid[neighbourRow, neighbourColumn] == 1)
                        liveNeighboursCount++;
                }
            }
        }

        return liveNeighboursCount;
    }

    //set the grid to the next generation based on the rules of the game
    public void nextGen(){
        int[,] newGrid = Grid.clearGrid(height, width);
        for(int row = 0; row < height; row++){
            for(int col = 0; col < width; col++){
                int neighbours = countLiveNeighbours(row, col);
                if (grid[row,col] == 1){
                    if (neighbours < 2 || neighbours > 3)
                        newGrid[row,col] = 0;
                    else
                        newGrid[row,col] = 1;
                }
                else{
                    if (neighbours == 3)
                        newGrid[row,col] = 1;
                    else
                        newGrid[row,col] = 0;
                }
            }
        }
        grid = newGrid;
    }

    public void eternalLife(){
       while (true){
            nextGen();
            WriteLine(this);
        }
    }

    public void mortalLife(int generations){
        for (int i = 0; i < generations; i++){
            nextGen();
            WriteLine(this);
        }
    }

    public override string ToString(){
        string output = "";
        for (int i = 0; i < height; i++){
            for (int j = 0; j < width; j++){
                if (grid[i,j] == 0)
                    output += " ";
                else
                    output += "#";
            }
            output += "\n";
        }
        return output;
    } 
}

// class Program {
//     static void Main1(){
//         GameOfLife game = new GameOfLife(10, 10);
//         game.mortalLife(10);
//     }
// } 