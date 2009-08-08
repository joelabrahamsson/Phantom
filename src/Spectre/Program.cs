﻿#region License

// Copyright 2009 Jeremy Skinner (http://www.jeremyskinner.co.uk)
//  
// Licensed under the Apache License, Version 2.0 (the "License"); 
// you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at 
//  
// http://www.apache.org/licenses/LICENSE-2.0 
//  
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, 
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
// See the License for the specific language governing permissions and 
// limitations under the License.
// 
// The latest version of this file can be found at http://github.com/JeremySkinner/Spectre

#endregion

namespace Spectre {
	using System;
	using System.Linq;
	using Boo.Lang.Useful.CommandLine;
	using Core;

	internal class Program {

		static void Main(string[] args) {
			var program = new Program();
			program.Execute(args);
		}

		void Execute(string[] args) {
			try {

				WriteHeader();

				var options = new SpectreOptions();

				try {
					options.Parse(args);
				}
				catch (CommandLineException exception) {
					Console.WriteLine(exception.Message);
					options.PrintOptions();
					return;
				}

				if(options.Help) {
					options.PrintOptions();
					return;
				}

				PrintTargets(options);

				var runner = new BuildRunner();
				runner.Execute(options);
			}
			catch(Exception exception) {
				Console.WriteLine(exception);
			}
		}

		void PrintTargets(SpectreOptions options) {
			string targets = string.Join(", ", options.TargetNames.ToArray());
			Console.WriteLine("Targets specified: {0}", targets);
			Console.WriteLine();
			Console.WriteLine();
		}

		void WriteHeader() {
			string version = typeof (Program).Assembly.GetName().Version.ToString();
			Console.WriteLine("Spectre v{0}", version);
			Console.WriteLine("Copyright (c) Jeremy Skinner 2009 (http://www.jeremyskinner.co.uk)");
			Console.WriteLine("http://github.com/JeremySkinner/Spectre");
			Console.WriteLine();
			Console.WriteLine();
		}
	}
}