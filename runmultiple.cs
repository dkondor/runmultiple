/*
 * runmultiple.cs
 * 
 * run a set of shell commands in parallel up to a maximum level of parallelism
 * 
 * Copyright 2020 Daniel Kondor <kondor.dani@gmail.com>
 * 
 * This is free and unencumbered software released into the public domain.
 * 
 */
using System;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

public class runmultiple {
	public const string usage = @"Usage: runmultiple [OPTIONS] [COMMAND] [ARGUMENTS]
Run multiple instances of COMMAND in parallel and wait for all of them to finish.

For each ARG in ARGUMENTS, runs COMMAND ARG. Multiple instances are run in
parallel, and waits for all of them to finish. By default, uses all available
cores in the computer. Instances of COMMAND are assigned to cores in a dynamic
fashion. Notably, if the number of instances to run is larger than the number
of cores, new instances are distributed among cores whenever one becomes
available.

Supported options:
  -t NUM        Use at maximum NUM cores instead of all available.
  -r MIN MAX    Instead of reading ARGUMENTS, generate a sequence of integers
                  between MIN and MAX (inclusive) and use these as arguments.
  -f FILE       Instead of reading COMMAND and ARGUMENTS, read a list of
                  commands to run from FILE. If FILE is '-', read from the
                  standard input.
  -s SHELL      Only to be used in combination with the -f argument. Set the
                  shell to use to run the commands read from FILE (default is
                  '/bin/sh'); SHELL must support the '-c' command line argument
                  to run a single command.
";
	
	public static void ShowHelp() {
		Console.Write(usage);
	}
	
	public static IEnumerable<string> readcmds(StreamReader r, char comment = '#') {
		while(!r.EndOfStream) {
			string s = r.ReadLine();
			if(s == null) continue;
			bool skip = true;
			foreach(char x in s) if(! (x == ' ' || x == '\t') ) {
				if(x != comment) skip = false;
				break;
			}
			if(!skip) yield return ("\"" + s.Replace("\"","\"\"") + "\"");
		}
		yield break;
	}
	
	public static void Main(string[] args) {
		string cmd = null;
		string[] args2 = null;
		string shell = "/bin/sh";
		string shellarg = "-c ";
		
		int min = 0;
		int max = 0;
		int nthreads = -1;
		
		string file = null;
		
		{
			int i = 0;
			for(;i<args.Length;i++) {
				if(args[i][0] == '-') switch(args[i][1]) {
					case 'r':
						min = Convert.ToInt32(args[i+1]);
						max = Convert.ToInt32(args[i+2]);
						i += 2;
						break;
					case 't':
						nthreads = Convert.ToInt32(args[i+1]);
						i++;
						break;
					case 'f':
						file = args[i+1];
						i++;
						break;
					case 's':
						shell = args[i+1];
						i++;
						break;
					case 'h':
						ShowHelp();
						return;
					default:
						Console.Error.WriteLine("Unknown argument: {0}!", args[i]);
						break;
				}
				else break;
			}
			
			if(file == null) {
				if(i == args.Length) throw new ArgumentException("No command name given!\n");
				cmd = args[i];
				i++;
				if(max <= min) {
					if(i == args.Length) throw new ArgumentException("No command arguments given!\n");
					args2 = new string[args.Length - i];
					Array.Copy(args,i,args2,0,args.Length-i);
				}
			}
		}
		
		ParallelOptions opt = new ParallelOptions();
		opt.MaxDegreeOfParallelism = nthreads;
		
		if(file != null) {
			/* read a file (or stdin), each line is one command */
			StreamReader sr = null;
			if(file == "-") sr = new StreamReader(Console.OpenStandardInput());
			else sr = new StreamReader(file);
			Parallel.ForEach(readcmds(sr),opt,(cmd1) => {
				ProcessStartInfo p = new ProcessStartInfo();
				p.FileName = shell;
				//~ p.ArgumentList.Add(shellarg);
				//~ p.ArgumentList.Add(cmd1);
				p.Arguments = shellarg + cmd1;
				p.CreateNoWindow = true;
				Process.Start(p).WaitForExit();
			});
		}
		else {
			if(args2 != null) {
				Parallel.ForEach(args2,opt,(arg) => {
					ProcessStartInfo p = new ProcessStartInfo();
					p.FileName = cmd;
					p.Arguments = arg;
					p.CreateNoWindow = true;
					Process.Start(p).WaitForExit();
				});
			}
			else {
				Parallel.For(min,max+1,opt,(i) => {
					ProcessStartInfo p = new ProcessStartInfo();
					p.FileName = cmd;
					p.Arguments = i.ToString();
					p.CreateNoWindow = true;
					Process.Start(p).WaitForExit();
				});
			}
		}
	}
}



