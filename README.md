# runmultiple
Simple C# wrapper to run a set of commands in parallel.

The main motivation is to be more flexible than typical shells' functionalities for parallelism.

### Compilation

```
mcs runmultiple.cs
```
or
```
csc runmultiple.cs
chmod +x runmultiple.exe
```
or just add it to a blank project if you're using Visual Studio or similar.

### Running

```
./runmultiple.exe COMMAND ARGS
```
or
```
mono runmultiple.exe COMMAND ARGS
```

### Simple examples

Compress all CSV files in the current directory with gzip using 6 threads / cores:
```
./runmultiple -t 6 gzip *.csv
```
Run all commands in the given file in parallel (using `/bin/sh -c`):
```
./runmultiple -f commands.sh
```


