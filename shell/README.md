# Shell

This folder contains the source files for a simple shell interface 
that allows running the other programs.

### Structure

The shell is currently hardcoded to run from a folder named 
**jos** in your home directory (~ in Linux/macOS, or Documents in
Windows). The binaries should be copied to **jos\bin** and `shell`
should be run from there. On startup, it will look for a home
folder under **jos\home** and create one if it is not found.

### Commands

| Command | Description                          |
|---------|--------------------------------------|
| DIR     | Display directory contents           |
| TYPE    | Display the contents of a file       |
| HELP    | Display a list of available commands |
| COMAL   | Run the Comal interpreter/compiler   |
| FORTRAN | Run the Fortran compiler             |
