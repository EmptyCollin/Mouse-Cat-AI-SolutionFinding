# Mouse-Cat-AI-SolutionFinding

This project is a practice of AI:
    the simulation of the process that cat(s) pursue the mouse(mice), and mouse(mice) eats the cheeses
    
      mouse(blue cube, moves to a adjacent grid each time, as the King in chess) 
                    is greedy which always chase to its nearest cheese - which means its path is fixed.
                    
      cat(red cylinder, moves to a catercorner gird in any 2*1 grids, as the Knight in chess) 
                    will try to catch all mouse before every cheeses are eaten 
                    - whose path is determined by the choosen searching method for searching tree
                    
      cheese(rotating yellow cube)
    
    this project tends to compare the performance difference among some searching method
    
Graphic careted by Unity 3D

note: don't try to add to much cats in the game, cause each cat will make the time complex be exponentially increased.
