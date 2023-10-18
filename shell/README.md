# Shell

This folder contains the source files for a very simple shell interface
that allows running the other programs and manipulating files.

To run the shell:

```
Shell 1.0.0
(c) Steven Palmer 2013-2023

$
```

On startup, it will look for a home folder under `..\home`
and create one if it is not found. The home folder then becomes the
default folder for all applications.

### Commands

| Command | Description                          |
|---------|--------------------------------------|
| COPY    | Make a copy of a file                |
| DIR     | Display directory contents           |
| TYPE    | Display the contents of a file       |
| DEL     | Delete files                         |
| EDIT    | Run the text editor                  |
| HELP    | Display a list of available commands |
| COMAL   | Run the Comal interpreter/compiler   |
| FOR     | Run the Fortran compiler             |
