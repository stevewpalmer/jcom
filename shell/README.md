# Shell

This folder contains the source files for a very simple shell interface 
that allows running the other programs and manipulating files.

### Structure

On startup, it will look for a home folder under `..\home` 
and create one if it is not found. The home folder then becomes the
default folder for all applications.

### Commands

| Command | Description                          |
|---------|--------------------------------------|
| DIR     | Display directory contents           |
| TYPE    | Display the contents of a file       |
| DEL     | Delete files                         |
| EDIT    | Run the text editor                  |
| HELP    | Display a list of available commands |
| COMAL   | Run the Comal interpreter/compiler   |
| FORTRAN | Run the Fortran compiler             |
