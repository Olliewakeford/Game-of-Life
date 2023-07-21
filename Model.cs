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
