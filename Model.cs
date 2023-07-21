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
}